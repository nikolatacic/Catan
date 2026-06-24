using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCore.Events;
using GameCore.Resources;

namespace Catan.UI
{
    // ── Scene setup ────────────────────────────────────────────────────────────
    // Place on a full-screen overlay panel (Canvas → DiscardPanel).
    // Set the panel inactive by default — Awake() enforces this.
    // Wire ConfirmButton.OnClick → OnConfirmDiscard() in the Inspector.
    // See scene layout guide below the class for hierarchy details.
    // ──────────────────────────────────────────────────────────────────────────

    public class DiscardPanelView : MonoBehaviour
    {
        [Header("Labels")]
        public TextMeshProUGUI PlayerNameLabel;
        public TextMeshProUGUI InstructionLabel;

        [Header("Card containers")]
        public Transform HandContainer;
        public Transform DiscardContainer;

        [Header("Confirm")]
        public Button ConfirmButton;

        [Header("Card prefab")]
        public GameObject CardPrefab;

        // ── Private state ──────────────────────────────────────────────────────

        private struct DiscardRequest { public CatanPlayer Player; public int Count; }

        private readonly Queue<DiscardRequest> _queue = new();
        private CatanPlayer _currentPlayer;
        private int _requiredCount;
        private readonly List<DiscardCardView> _handCards = new();
        private readonly List<DiscardCardView> _selectedCards = new();

        // ── Unity lifecycle ────────────────────────────────────────────────────

        private void Awake()
        {
            gameObject.SetActive(false);
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

            if (!gameObject.activeSelf)
                ProcessNext();
        }

        // ── Flow ───────────────────────────────────────────────────────────────

        private void ProcessNext()
        {
            if (_queue.Count == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            var request = _queue.Dequeue();
            _currentPlayer = request.Player;
            _requiredCount = request.Count;

            gameObject.SetActive(true);
            PopulateHand();
            RefreshUI();
        }

        private void PopulateHand()
        {
            ClearCards();

            var resources = _currentPlayer.Resources.Current;
            IResource[] allResources =
            {
                CatanResources.Wood, CatanResources.Brick,
                CatanResources.Sheep, CatanResources.Wheat, CatanResources.Ore
            };

            foreach (var resource in allResources)
            {
                int count = resources.Get(resource);
                for (int cardIndex = 0; cardIndex < count; cardIndex++)
                    SpawnCard(resource, HandContainer, _handCards);
            }
        }

        private void SpawnCard(IResource resource, Transform container, List<DiscardCardView> targetList)
        {
            if (CardPrefab == null) return;

            var go = Instantiate(CardPrefab, container);
            var cardView = go.GetComponent<DiscardCardView>();
            if (cardView == null) return;

            cardView.Initialize(resource, OnCardClicked);
            targetList.Add(cardView);
        }

        private void OnCardClicked(DiscardCardView card)
        {
            if (_handCards.Remove(card))
            {
                card.transform.SetParent(DiscardContainer, false);
                _selectedCards.Add(card);
            }
            else if (_selectedCards.Remove(card))
            {
                card.transform.SetParent(HandContainer, false);
                _handCards.Add(card);
            }

            RefreshUI();
        }

        // ── Confirm ────────────────────────────────────────────────────────────

        public void OnConfirmDiscard()
        {
            if (_selectedCards.Count != _requiredCount) return;

            var discardBundle = new ResourceBundle();
            foreach (var card in _selectedCards)
                discardBundle = discardBundle.Add(card.Resource, 1);

            _currentPlayer.Resources.TryRemove(discardBundle);

            ClearCards();
            ProcessNext();
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void RefreshUI()
        {
            if (PlayerNameLabel != null)
                PlayerNameLabel.text = _currentPlayer?.DisplayName ?? "";

            if (InstructionLabel != null)
                InstructionLabel.text =
                    $"Select {_requiredCount} cards to discard  ({_selectedCards.Count}/{_requiredCount})";

            if (ConfirmButton != null)
                ConfirmButton.interactable = _selectedCards.Count == _requiredCount;
        }

        private void ClearCards()
        {
            foreach (var card in _handCards)   if (card != null) Destroy(card.gameObject);
            foreach (var card in _selectedCards) if (card != null) Destroy(card.gameObject);
            _handCards.Clear();
            _selectedCards.Clear();
        }
    }
}
