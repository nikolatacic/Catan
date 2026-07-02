using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using GameCore.Events;
using GameCore.Resources;
using GameCore.Score;
using GameCore.Turn;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class AllPlayersSummaryView : MonoBehaviour
    {
        private VisualElement _summaryRowContainer;
        private readonly List<PlayerSummaryRowView> _summaryRows = new();
        private CatanPlayer _activePlayer;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _summaryRowContainer = root.Q<VisualElement>("SummaryRowContainer");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Subscribe<ResourceAddedEvent>(OnResourceAdded);
            EventBus.Subscribe<ResourceRemovedEvent>(OnResourceRemoved);
            EventBus.Subscribe<KnightPlayedEvent>(OnKnightPlayed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Unsubscribe<ResourceAddedEvent>(OnResourceAdded);
            EventBus.Unsubscribe<ResourceRemovedEvent>(OnResourceRemoved);
            EventBus.Unsubscribe<KnightPlayedEvent>(OnKnightPlayed);
        }

        private void OnTurnStarted(TurnStartedEvent gameEvent)
        {
            _activePlayer = gameEvent.Actor as CatanPlayer;
            if (_summaryRows.Count == 0)
                BuildRows();
            RefreshAll();
        }

        private void OnScoreChanged(ScoreChangedEvent gameEvent) => RefreshAll();
        private void OnResourceAdded(ResourceAddedEvent gameEvent) => RefreshSingleRow(gameEvent.Player as CatanPlayer);
        private void OnResourceRemoved(ResourceRemovedEvent gameEvent) => RefreshSingleRow(gameEvent.Player as CatanPlayer);
        private void OnKnightPlayed(KnightPlayedEvent gameEvent) => RefreshSingleRow(gameEvent.Player as CatanPlayer);

        private void BuildRows()
        {
            var manager = GameManager.Instance;
            if (manager == null || _summaryRowContainer == null) return;

            _summaryRowContainer.Clear();
            _summaryRows.Clear();

            foreach (var player in manager.Players)
            {
                var catanPlayer = player as CatanPlayer;
                if (catanPlayer == null) continue;
                var summaryRow = new PlayerSummaryRowView(catanPlayer);
                _summaryRowContainer.Add(summaryRow.Root);
                _summaryRows.Add(summaryRow);
            }
        }

        private void RefreshAll()
        {
            var manager = GameManager.Instance;
            foreach (var summaryRow in _summaryRows)
            {
                if (summaryRow.Player == null) continue;
                int playerScore = manager?.ScoreManager.GetScore(summaryRow.Player) ?? 0;
                summaryRow.Refresh(isActive: summaryRow.Player == _activePlayer, score: playerScore);
            }
        }

        private void RefreshSingleRow(CatanPlayer player)
        {
            if (player == null) return;
            var manager = GameManager.Instance;
            foreach (var summaryRow in _summaryRows)
            {
                if (summaryRow.Player != player) continue;
                int playerScore = manager?.ScoreManager.GetScore(player) ?? 0;
                summaryRow.Refresh(isActive: player == _activePlayer, score: playerScore);
                break;
            }
        }
    }
}
