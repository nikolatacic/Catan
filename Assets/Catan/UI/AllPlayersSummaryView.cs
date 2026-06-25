using System.Collections.Generic;
using UnityEngine;
using GameCore.Events;
using GameCore.Resources;
using GameCore.Turn;
using GameCore.Score;

namespace Catan.UI
{
    // ── Scene setup ────────────────────────────────────────────────────────────
    // Place anywhere on Canvas (side panel recommended). Assign RowContainer
    // and RowPrefab. Rows are spawned automatically from GameManager.Players
    // on the first TurnStartedEvent — no manual wiring per player needed.
    // ──────────────────────────────────────────────────────────────────────────

    public class AllPlayersSummaryView : MonoBehaviour
    {
        [Header("Layout")]
        public Transform RowContainer;
        public GameObject RowPrefab;

        private readonly List<PlayerSummaryRowView> _rows = new();
        private CatanPlayer _activePlayer;

        private void OnEnable()
        {
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Subscribe<ResourceAddedEvent>(OnResourceChanged);
            EventBus.Subscribe<ResourceRemovedEvent>(OnResourceChanged);
            EventBus.Subscribe<KnightPlayedEvent>(OnKnightPlayed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Unsubscribe<ResourceAddedEvent>(OnResourceChanged);
            EventBus.Unsubscribe<ResourceRemovedEvent>(OnResourceChanged);
            EventBus.Unsubscribe<KnightPlayedEvent>(OnKnightPlayed);
        }

        // ── Event handlers ─────────────────────────────────────────────────────

        private void OnTurnStarted(TurnStartedEvent gameEvent)
        {
            _activePlayer = gameEvent.Actor as CatanPlayer;

            if (_rows.Count == 0)
                BuildRows();

            RefreshAll();
        }

        private void OnScoreChanged(ScoreChangedEvent gameEvent) => RefreshAll();
        private void OnResourceChanged(ResourceAddedEvent gameEvent) => RefreshRow(gameEvent.Player as CatanPlayer);
        private void OnResourceChanged(ResourceRemovedEvent gameEvent) => RefreshRow(gameEvent.Player as CatanPlayer);
        private void OnKnightPlayed(KnightPlayedEvent gameEvent) => RefreshRow(gameEvent.Player as CatanPlayer);

        // ── Row management ─────────────────────────────────────────────────────

        private void BuildRows()
        {
            var manager = GameManager.Instance;
            if (manager == null || RowPrefab == null) return;

            foreach (var player in manager.Players)
            {
                var go = Instantiate(RowPrefab, RowContainer);
                var row = go.GetComponent<PlayerSummaryRowView>();
                if (row == null) continue;
                row.Initialize(player);
                _rows.Add(row);
            }
        }

        private void RefreshAll()
        {
            var manager = GameManager.Instance;
            foreach (var row in _rows)
            {
                if (row == null || row.Player == null) continue;
                int score = manager?.ScoreManager.GetScore(row.Player) ?? 0;
                row.Refresh(isActive: row.Player == _activePlayer, score: score);
            }
        }

        private void RefreshRow(CatanPlayer player)
        {
            if (player == null) return;
            var manager = GameManager.Instance;
            foreach (var row in _rows)
            {
                if (row.Player != player) continue;
                int score = manager?.ScoreManager.GetScore(player) ?? 0;
                row.Refresh(isActive: player == _activePlayer, score: score);
                break;
            }
        }
    }
}
