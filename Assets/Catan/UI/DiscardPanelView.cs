using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using GameCore.Events;
using GameCore.Resources;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class DiscardPanelView : MonoBehaviour
    {
        private struct DiscardRequest { public CatanPlayer Player; public int Count; }

        private readonly Queue<DiscardRequest> _queue = new();
        private CatanPlayer _currentPlayer;
        private int _requiredCount;
        private readonly List<DiscardCardView> _handCards     = new();
        private readonly List<DiscardCardView> _selectedCards = new();

        private VisualElement _panelRoot;
        private Label         _playerNameLabel;
        private Label         _instructionLabel;
        private ScrollView    _handScrollView;
        private ScrollView    _discardScrollView;
        private Button        _confirmButton;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _panelRoot        = root.Q<VisualElement>("DiscardPanelRoot");
            _playerNameLabel  = root.Q<Label>("DiscardPlayerNameLabel");
            _instructionLabel = root.Q<Label>("DiscardInstructionLabel");
            _handScrollView    = root.Q<ScrollView>("DiscardHandContainer");
            _discardScrollView = root.Q<ScrollView>("DiscardSelectedContainer");
            _confirmButton    = root.Q<Button>("DiscardConfirmButton");

            _confirmButton?.RegisterCallback<ClickEvent>(_ => OnConfirmDiscard());

            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.None;

            EventBus.Subscribe<DiscardRequiredEvent>(OnDiscardRequired);
        }


        private void OnDestroy()
        {
            EventBus.Unsubscribe<DiscardRequiredEvent>(OnDiscardRequired);
        }

        // ── Event handling ─────────────────────────────────────────────────────

        private void OnDiscardRequired(DiscardRequiredEvent gameEvent)
        {
            if (gameEvent.Player is not CatanPlayer catanPlayer) return;

            _queue.Enqueue(new DiscardRequest { Player = catanPlayer, Count = gameEvent.Count });

            bool panelIsCurrentlyHidden = _panelRoot == null
                || _panelRoot.style.display == DisplayStyle.None;

            if (panelIsCurrentlyHidden)
                ProcessNext();
        }

        // ── Flow ───────────────────────────────────────────────────────────────

        private void ProcessNext()
        {
            if (_queue.Count == 0)
            {
                if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.None;
                return;
            }

            var discardRequest = _queue.Dequeue();
            _currentPlayer = discardRequest.Player;
            _requiredCount = discardRequest.Count;

            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.Flex;
            PopulateHand();
            RefreshUI();
        }

        private void PopulateHand()
        {
            ClearCards();

            var currentResources = _currentPlayer.Resources.Current;
            IResource[] allResources =
            {
                CatanResources.Wood, CatanResources.Brick,
                CatanResources.Sheep, CatanResources.Wheat, CatanResources.Ore
            };

            foreach (var resource in allResources)
            {
                int cardCount = currentResources.Get(resource);
                for (int cardIndex = 0; cardIndex < cardCount; cardIndex++)
                    SpawnHandCard(resource);
            }
        }

        private void SpawnHandCard(IResource resource)
        {
            var discardCard = new DiscardCardView(resource, OnCardClicked);
            _handCards.Add(discardCard);
            _handScrollView?.Add(discardCard.Root);
        }

        private void OnCardClicked(DiscardCardView clickedCard)
        {
            if (_handCards.Remove(clickedCard))
            {
                clickedCard.Root.RemoveFromHierarchy();
                _discardScrollView?.Add(clickedCard.Root);
                _selectedCards.Add(clickedCard);
            }
            else if (_selectedCards.Remove(clickedCard))
            {
                clickedCard.Root.RemoveFromHierarchy();
                _handScrollView?.Add(clickedCard.Root);
                _handCards.Add(clickedCard);
            }

            RefreshUI();
        }

        // ── Confirm ────────────────────────────────────────────────────────────

        private void OnConfirmDiscard()
        {
            if (_selectedCards.Count != _requiredCount) return;

            var discardBundle = new ResourceBundle();
            foreach (var discardCard in _selectedCards)
                discardBundle = discardBundle.Add(discardCard.Resource, 1);

            _currentPlayer.Resources.TryRemove(discardBundle);

            ClearCards();
            ProcessNext();
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void RefreshUI()
        {
            if (_playerNameLabel != null)
                _playerNameLabel.text = _currentPlayer?.DisplayName ?? "";

            if (_instructionLabel != null)
                _instructionLabel.text =
                    $"Select {_requiredCount} cards to discard  ({_selectedCards.Count}/{_requiredCount})";

            _confirmButton?.SetEnabled(_selectedCards.Count == _requiredCount);
        }

        private void ClearCards()
        {
            foreach (var handCard in _handCards)
                handCard.Root.RemoveFromHierarchy();

            foreach (var selectedCard in _selectedCards)
                selectedCard.Root.RemoveFromHierarchy();

            _handCards.Clear();
            _selectedCards.Clear();
        }
    }
}
