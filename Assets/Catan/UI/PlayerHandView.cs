using UnityEngine;
using TMPro;
using GameCore.Events;
using GameCore.Resources;

namespace Catan.UI
{
    // ── Scene setup ────────────────────────────────────────────────────────────
    // One instance on Canvas. Automatically follows the active player via
    // TurnStartedEvent — no PlayerIndex needed.
    //
    // Wire 5 ResourceCardSlotView children (Wood/Brick/Sheep/Wheat/Ore) to the
    // corresponding slot fields. Each slot has its CatanResource assigned in
    // the slot's own Inspector.
    // ──────────────────────────────────────────────────────────────────────────

    public class PlayerHandView : MonoBehaviour
    {
        [Header("Player info")]
        public TextMeshProUGUI PlayerNameLabel;
        public TextMeshProUGUI TotalCardsLabel;
        public TextMeshProUGUI VictoryPointsLabel;

        [Header("Resource card slots (one per type)")]
        public ResourceCardSlotView WoodSlot;
        public ResourceCardSlotView BrickSlot;
        public ResourceCardSlotView SheepSlot;
        public ResourceCardSlotView WheatSlot;
        public ResourceCardSlotView OreSlot;

        private CatanPlayer _player;

        private void OnEnable()
        {
            EventBus.Subscribe<ResourceAddedEvent>(OnResourceAdded);
            EventBus.Subscribe<ResourceRemovedEvent>(OnResourceRemoved);
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Subscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ResourceAddedEvent>(OnResourceAdded);
            EventBus.Unsubscribe<ResourceRemovedEvent>(OnResourceRemoved);
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Unsubscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
        }

        private void OnTurnStarted(GameCore.Turn.TurnStartedEvent gameEvent)
        {
            _player = gameEvent.Actor as CatanPlayer;
            Refresh();
        }

        private void OnResourceAdded(ResourceAddedEvent gameEvent)
        {
            if (gameEvent.Player == _player) Refresh();
        }

        private void OnResourceRemoved(ResourceRemovedEvent gameEvent)
        {
            if (gameEvent.Player == _player) Refresh();
        }

        private void OnScoreChanged(ScoreChangedEvent gameEvent)
        {
            if (gameEvent.Player == _player) RefreshScore();
        }

        public void Refresh()
        {
            if (_player == null) return;

            if (PlayerNameLabel != null)
                PlayerNameLabel.text = _player.DisplayName;

            var resources = _player.Resources.Current;

            WoodSlot?.Refresh(resources.Get(CatanResources.Wood));
            BrickSlot?.Refresh(resources.Get(CatanResources.Brick));
            SheepSlot?.Refresh(resources.Get(CatanResources.Sheep));
            WheatSlot?.Refresh(resources.Get(CatanResources.Wheat));
            OreSlot?.Refresh(resources.Get(CatanResources.Ore));

            int totalCards = resources.Get(CatanResources.Wood)
                           + resources.Get(CatanResources.Brick)
                           + resources.Get(CatanResources.Sheep)
                           + resources.Get(CatanResources.Wheat)
                           + resources.Get(CatanResources.Ore);

            if (TotalCardsLabel != null)
                TotalCardsLabel.text = $"Cards: {totalCards}";

            RefreshScore();
        }

        private void RefreshScore()
        {
            if (_player == null || VictoryPointsLabel == null) return;
            var manager = GameManager.Instance;
            if (manager == null) return;
            VictoryPointsLabel.text = $"VP: {manager.ScoreManager.GetScore(_player)}";
        }
    }
}
