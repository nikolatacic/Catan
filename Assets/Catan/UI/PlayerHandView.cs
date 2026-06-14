using UnityEngine;
using TMPro;
using GameCore.Events;
using GameCore.Resources;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Attach to a UI panel. Assign ResourceLabels (5 elements: Wood/Brick/Sheep/Wheat/Ore).
    // Set PlayerIndex to which player (0-based) this panel displays.
    // ──────────────────────────────────────────────────────────────────────────

    public class PlayerHandView : MonoBehaviour
    {
        [Header("Player")]
        public int PlayerIndex = 0;

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

        private void Start()
        {
            ResolvePlayer();
            Refresh();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<ResourceAddedEvent>(OnResourceChanged);
            EventBus.Subscribe<ResourceRemovedEvent>(OnResourceChanged);
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Subscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ResourceAddedEvent>(OnResourceChanged);
            EventBus.Unsubscribe<ResourceRemovedEvent>(OnResourceChanged);
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Unsubscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
        }

        private void OnResourceChanged(ResourceAddedEvent gameEvent) => RefreshIfOwner(gameEvent.Player);
        private void OnResourceChanged(ResourceRemovedEvent gameEvent) => RefreshIfOwner(gameEvent.Player);

        private void OnScoreChanged(ScoreChangedEvent gameEvent)
        {
            if (gameEvent.Player == _player) RefreshScore();
        }

        private void OnTurnStarted(GameCore.Turn.TurnStartedEvent gameEvent) => Refresh();

        private void RefreshIfOwner(GameCore.Player.IPlayer owner)
        {
            if (owner == _player) Refresh();
        }

        public void Refresh()
        {
            ResolvePlayer();
            if (_player == null) return;

            if (PlayerNameLabel != null)
                PlayerNameLabel.text = _player.DisplayName;

            var resources = _player.Resources.Current;

            if (WoodLabel != null) WoodLabel.text = resources.Get(CatanResources.Wood).ToString();
            if (BrickLabel != null) BrickLabel.text = resources.Get(CatanResources.Brick).ToString();
            if (SheepLabel != null) SheepLabel.text = resources.Get(CatanResources.Sheep).ToString();
            if (WheatLabel != null) WheatLabel.text = resources.Get(CatanResources.Wheat).ToString();
            if (OreLabel != null) OreLabel.text = resources.Get(CatanResources.Ore).ToString();

            int totalCards = resources.Get(CatanResources.Wood)
                           + resources.Get(CatanResources.Brick)
                           + resources.Get(CatanResources.Sheep)
                           + resources.Get(CatanResources.Wheat)
                           + resources.Get(CatanResources.Ore);

            if (TotalCardsLabel != null) TotalCardsLabel.text = $"Cards: {totalCards}";
            if (DevCardsLabel != null) DevCardsLabel.text = $"Dev: {_player.DevelopmentCards.Cards.Count}";

            RefreshScore();
        }

        private void RefreshScore()
        {
            if (_player == null) return;
            var manager = GameManager.Instance;
            if (manager == null || VictoryPointsLabel == null) return;

            int score = manager.ScoreManager.GetScore(_player);
            VictoryPointsLabel.text = $"VP: {score}";
        }

        private void ResolvePlayer()
        {
            if (_player != null) return;
            var manager = GameManager.Instance;
            if (manager == null || PlayerIndex >= manager.Players.Count) return;
            _player = manager.Players[PlayerIndex];
        }
    }
}
