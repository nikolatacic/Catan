using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GameCore.Events;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Attach to a UI panel. Assign all label fields in the Inspector.
    // ──────────────────────────────────────────────────────────────────────────

    public class TurnIndicatorView : MonoBehaviour
    {
        [Header("Labels")]
        public TextMeshProUGUI ActivePlayerLabel;
        public TextMeshProUGUI PhaseLabel;
        public TextMeshProUGUI TurnNumberLabel;
        public Image PlayerColorIndicator;

        private void OnEnable()
        {
            EventBus.Subscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
        }

        private void Start() => Refresh();

        private void OnTurnStarted(GameCore.Turn.TurnStartedEvent gameEvent) => Refresh();
        private void OnPhaseChanged(CatanPhaseChangedEvent gameEvent) => RefreshPhase(gameEvent.To);

        private void Refresh()
        {
            var manager = GameManager.Instance;
            if (manager == null) return;

            var player = manager.ActivePlayer;
            if (ActivePlayerLabel != null)
                ActivePlayerLabel.text = player != null ? player.DisplayName : "—";

            if (PlayerColorIndicator != null && player != null)
                PlayerColorIndicator.color = player.Color;

            if (TurnNumberLabel != null)
                TurnNumberLabel.text = $"Turn {manager.TurnManager.TurnNumber}";

            RefreshPhase(manager.TurnManager.CurrentCatanPhase);
        }

        private void RefreshPhase(CatanTurnPhase phase)
        {
            if (PhaseLabel == null) return;
            PhaseLabel.text = phase switch
            {
                CatanTurnPhase.SetupPlacement => "Setup — Place Settlement & Road",
                CatanTurnPhase.RollDice       => "Roll Dice",
                CatanTurnPhase.Robber         => "Move Robber",
                CatanTurnPhase.Trading        => "Trading",
                CatanTurnPhase.Building       => "Building",
                CatanTurnPhase.EndTurn        => "End Turn",
                _                             => phase.ToString()
            };
        }
    }
}
