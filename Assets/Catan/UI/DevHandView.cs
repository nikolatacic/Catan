using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using GameCore.Cards;
using GameCore.Events;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class DevHandView : MonoBehaviour
    {
        [Serializable]
        public struct CardSpriteEntry
        {
            public string CardId;
            public Sprite Sprite;
        }

        [Header("Card sprites — one entry per DevelopmentCard.CardId (e.g. \"Knight\")")]
        public List<CardSpriteEntry> CardSprites = new();

        private VisualElement _devCardContainer;
        private Label _emptyLabel;
        private CatanPlayer _trackedPlayer;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _devCardContainer = root.Q<VisualElement>("DevCardContainer");
            _emptyLabel       = root.Q<Label>("DevHandEmptyLabel");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<DevCardPurchasedEvent>(OnDevCardPurchased);
            EventBus.Subscribe<CardPlayedEvent<DevelopmentCard>>(OnCardPlayed);
            EventBus.Subscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Subscribe<LocalPlayerAssignedEvent>(OnLocalPlayerAssigned);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<DevCardPurchasedEvent>(OnDevCardPurchased);
            EventBus.Unsubscribe<CardPlayedEvent<DevelopmentCard>>(OnCardPlayed);
            EventBus.Unsubscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Unsubscribe<LocalPlayerAssignedEvent>(OnLocalPlayerAssigned);
        }

        private void OnTurnStarted(GameCore.Turn.TurnStartedEvent gameEvent)
        {
            ResolveTrackedPlayer(gameEvent.Actor as CatanPlayer);
            Rebuild();
        }

        private void OnLocalPlayerAssigned(LocalPlayerAssignedEvent gameEvent)
        {
            ResolveTrackedPlayer(activePlayer: null);
            Rebuild();
        }

        // Hotseat: track the active player. Networked: track the local player.
        private void ResolveTrackedPlayer(CatanPlayer activePlayer)
        {
            if (Catan.NetworkSession.IsNetworked)
            {
                var manager = GameManager.Instance;
                int localPlayerIndex = Catan.NetworkSession.LocalPlayerIndex;
                if (manager != null && localPlayerIndex >= 0 && localPlayerIndex < manager.Players.Count)
                    _trackedPlayer = manager.Players[localPlayerIndex];
            }
            else
            {
                _trackedPlayer = activePlayer ?? _trackedPlayer;
            }
        }

        private void OnDevCardPurchased(DevCardPurchasedEvent gameEvent)
        {
            if (gameEvent.Player == _trackedPlayer) Rebuild();
        }

        private void OnCardPlayed(CardPlayedEvent<DevelopmentCard> gameEvent) => Rebuild();
        private void OnPhaseChanged(CatanPhaseChangedEvent gameEvent) => Rebuild();

        private void Rebuild()
        {
            if (_devCardContainer == null) return;

            _devCardContainer.Clear();

            bool hasCards = _trackedPlayer != null && _trackedPlayer.DevelopmentCards.Cards.Count > 0;

            if (_emptyLabel != null)
                _emptyLabel.style.display = hasCards ? DisplayStyle.None : DisplayStyle.Flex;

            if (!hasCards) return;

            var manager = GameManager.Instance;
            CatanGameContext gameContext = null;
            if (manager != null)
            {
                gameContext = new CatanGameContext(
                    manager.ActivePlayer,
                    manager.TurnManager,
                    manager.Board,
                    new List<GameCore.Player.IPlayer>(manager.Players));
            }

            foreach (var developmentCard in _trackedPlayer.DevelopmentCards.Cards)
            {
                bool isPlayable = gameContext != null && developmentCard.IsPlayable(gameContext);
                var cardItemView = new DevCardItemView(developmentCard, isPlayable, GetCardSprite(developmentCard.CardId));
                _devCardContainer.Add(cardItemView.Root);
            }
        }

        private Sprite GetCardSprite(string cardId)
        {
            foreach (var spriteEntry in CardSprites)
            {
                if (spriteEntry.CardId == cardId)
                    return spriteEntry.Sprite;
            }
            return null;
        }
    }
}
