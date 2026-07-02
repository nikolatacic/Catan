using UnityEngine;
using UnityEngine.UIElements;
using GameCore.Events;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class TurnIndicatorView : MonoBehaviour
    {
        private Label _activePlayerLabel;
        private Label _turnNumberLabel;
        private Label _phaseLabel;
        private Label _diceResultLabel;
        private VisualElement _playerColorBar;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _activePlayerLabel = root.Q<Label>("ActivePlayerLabel");
            _turnNumberLabel   = root.Q<Label>("TurnNumberLabel");
            _phaseLabel        = root.Q<Label>("PhaseLabel");
            _diceResultLabel   = root.Q<Label>("DiceResultLabel");
            _playerColorBar    = root.Q<VisualElement>("PlayerColorBar");
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Subscribe<DiceRolledEvent>(OnDiceRolled);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
        }

        private void Start() => Refresh();

        private void OnTurnStarted(GameCore.Turn.TurnStartedEvent gameEvent)
        {
            if (_diceResultLabel != null) _diceResultLabel.text = "";
            Refresh();
        }

        private void OnPhaseChanged(CatanPhaseChangedEvent gameEvent) => RefreshPhase(gameEvent.To);

        private void OnDiceRolled(DiceRolledEvent gameEvent)
        {
            if (_diceResultLabel != null)
                _diceResultLabel.text = $"{gameEvent.D1} + {gameEvent.D2} = {gameEvent.Total}";
        }

        private void Refresh()
        {
            var manager = GameManager.Instance;
            if (manager == null) return;

            var activePlayer = manager.ActivePlayer;

            if (_activePlayerLabel != null)
                _activePlayerLabel.text = activePlayer != null ? activePlayer.DisplayName : "—";

            if (_playerColorBar != null && activePlayer != null)
            {
                var playerColor = activePlayer.Color;
                _playerColorBar.style.backgroundColor = new StyleColor(playerColor);
            }

            if (_turnNumberLabel != null)
                _turnNumberLabel.text = $"Turn {manager.TurnManager.TurnNumber}";

            RefreshPhase(manager.TurnManager.CurrentCatanPhase);
        }

        private void RefreshPhase(CatanTurnPhase phase)
        {
            if (_phaseLabel == null) return;
            _phaseLabel.text = phase switch
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
