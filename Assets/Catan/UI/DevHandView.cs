using System.Collections.Generic;
using UnityEngine;
using GameCore.Cards;
using GameCore.Events;

namespace Catan.UI
{
    // ── Scene setup ────────────────────────────────────────────────────────────
    // Place anywhere on Canvas (side panel recommended).
    // Assign CardContainer (Transform) — a VerticalLayoutGroup works well.
    // Assign DevCardItemPrefab — a prefab with DevCardItemView component.
    // Automatically follows the active player via TurnStartedEvent.
    // ──────────────────────────────────────────────────────────────────────────

    public class DevHandView : MonoBehaviour
    {
        public Transform CardContainer;
        public GameObject DevCardItemPrefab;

        private CatanPlayer _player;

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
            ResolvePlayer(gameEvent.Actor as CatanPlayer);
            Rebuild();
        }

        private void OnLocalPlayerAssigned(LocalPlayerAssignedEvent gameEvent)
        {
            ResolvePlayer(activePlayer: null);
            Rebuild();
        }

        // Hotseat: track the active player. Networked: track the local player.
        private void ResolvePlayer(CatanPlayer activePlayer)
        {
            if (Catan.NetworkSession.IsNetworked)
            {
                var manager = GameManager.Instance;
                int index = Catan.NetworkSession.LocalPlayerIndex;
                if (manager != null && index >= 0 && index < manager.Players.Count)
                    _player = manager.Players[index];
            }
            else
            {
                _player = activePlayer ?? _player;
            }
        }

        private void OnDevCardPurchased(DevCardPurchasedEvent gameEvent)
        {
            if (gameEvent.Player == _player) Rebuild();
        }

        private void OnCardPlayed(CardPlayedEvent<DevelopmentCard> gameEvent) => Rebuild();

        private void OnPhaseChanged(CatanPhaseChangedEvent gameEvent) => Rebuild();

        private void Rebuild()
        {
            if (CardContainer == null || DevCardItemPrefab == null) return;

            foreach (Transform child in CardContainer)
                Destroy(child.gameObject);

            if (_player == null) return;

            var manager = GameManager.Instance;
            CatanGameContext context = null;
            if (manager != null)
            {
                context = new CatanGameContext(
                    manager.ActivePlayer,
                    manager.TurnManager,
                    manager.Board,
                    new List<GameCore.Player.IPlayer>(manager.Players));
            }

            foreach (var card in _player.DevelopmentCards.Cards)
            {
                var go = Instantiate(DevCardItemPrefab, CardContainer);
                var itemView = go.GetComponent<DevCardItemView>();
                if (itemView == null) continue;

                bool isPlayable = context != null && card.IsPlayable(context);
                itemView.Initialize(card, isPlayable);
            }
        }
    }
}
