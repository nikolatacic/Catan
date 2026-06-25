using UnityEngine;
using TMPro;
using GameCore.Events;
using GameCore.Resources;
using GameCore.Player;
using GameCore.Score;

namespace Catan.UI
{
    // ── Scene setup ────────────────────────────────────────────────────────────
    // One instance on Canvas. No PlayerIndex needed — automatically follows
    // whoever's turn it is via TurnStartedEvent.
    // ──────────────────────────────────────────────────────────────────────────

    public class PlayerHandView : MonoBehaviour
    {
        [Header("Labels")]
        public TextMeshProUGUI PlayerNameLabel;
        public TextMeshProUGUI WoodLabel;
        public TextMeshProUGUI BrickLabel;
        public TextMeshProUGUI SheepLabel;
        public TextMeshProUGUI WheatLabel;
        public TextMeshProUGUI OreLabel;
        public TextMeshProUGUI TotalCardsLabel;
        public TextMeshProUGUI VictoryPointsLabel;
        public TextMeshProUGUI DevCardsLabel;

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
            if (WoodLabel  != null) WoodLabel.text  = resources.Get(CatanResources.Wood).ToString();
            if (BrickLabel != null) BrickLabel.text = resources.Get(CatanResources.Brick).ToString();
            if (SheepLabel != null) SheepLabel.text = resources.Get(CatanResources.Sheep).ToString();
            if (WheatLabel != null) WheatLabel.text = resources.Get(CatanResources.Wheat).ToString();
            if (OreLabel   != null) OreLabel.text   = resources.Get(CatanResources.Ore).ToString();

            int totalCards = resources.Get(CatanResources.Wood)
                           + resources.Get(CatanResources.Brick)
                           + resources.Get(CatanResources.Sheep)
                           + resources.Get(CatanResources.Wheat)
                           + resources.Get(CatanResources.Ore);

            if (TotalCardsLabel != null) TotalCardsLabel.text = $"Cards: {totalCards}";
            if (DevCardsLabel   != null) DevCardsLabel.text   = $"Dev: {_player.DevelopmentCards.Cards.Count}";

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
