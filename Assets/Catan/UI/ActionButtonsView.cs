using UnityEngine;
using UnityEngine.UI;
using GameCore.Events;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Attach to a UI panel. Wire each Button field and call the corresponding
    // OnXxx() method from the Button's OnClick event.
    // ──────────────────────────────────────────────────────────────────────────

    public class ActionButtonsView : MonoBehaviour
    {
        [Header("Turn buttons")]
        public Button RollDiceButton;
        public Button EndTurnButton;

        [Header("Build buttons")]
        public Button BuildSettlementButton;
        public Button BuildRoadButton;
        public Button BuildCityButton;
        public Button BuyDevCardButton;
        public Button CancelPlacementButton;

        private void OnEnable()
        {
            EventBus.Subscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Subscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<GameCore.Build.BuildSucceededEvent>(OnBuildSucceeded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Unsubscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<GameCore.Build.BuildSucceededEvent>(OnBuildSucceeded);
        }

        private void Start() => RefreshButtons();

        private void OnPhaseChanged(CatanPhaseChangedEvent gameEvent) => RefreshButtons();
        private void OnTurnStarted(GameCore.Turn.TurnStartedEvent gameEvent) => RefreshButtons();
        private void OnBuildSucceeded(GameCore.Build.BuildSucceededEvent gameEvent) => RefreshButtons();

        // ── Button callbacks ───────────────────────────────────────────────────

        public void OnRollDice()      => GameManager.Instance?.RequestRoll();
        public void OnEndTurn()       => GameManager.Instance?.EndTurn();
        public void OnBuildSettlement() => GameManager.Instance?.BeginPlaceSettlement();
        public void OnBuildRoad()     => GameManager.Instance?.BeginPlaceRoad();
        public void OnBuildCity()     => GameManager.Instance?.BeginUpgradeCity();
        public void OnCancelPlacement() => GameManager.Instance?.CancelPlacement();

        public void OnBuyDevCard()
        {
            GameManager.Instance?.TryPurchaseDevCard();
        }

        // ── Interactability ────────────────────────────────────────────────────

        private void RefreshButtons()
        {
            var manager = GameManager.Instance;
            if (manager == null)
            {
                SetAllInteractable(false);
                return;
            }

            var phase = manager.TurnManager.CurrentCatanPhase;
            var player = manager.ActivePlayer;

            bool isRollPhase     = phase == CatanTurnPhase.RollDice;
            bool isBuildPhase    = phase == CatanTurnPhase.Building;
            bool isTradingPhase  = phase == CatanTurnPhase.Trading;
            bool isEndTurnPhase  = phase == CatanTurnPhase.EndTurn;
            bool isSetupPhase    = phase == CatanTurnPhase.SetupPlacement;
            bool canBuild        = isBuildPhase || isSetupPhase;
            bool canEndTurn      = isBuildPhase || isEndTurnPhase;

            SetInteractable(RollDiceButton,         isRollPhase);
            SetInteractable(EndTurnButton,          canEndTurn);
            bool setupSettlementPlaced = manager.SetupSettlementPlaced;
            SetInteractable(BuildSettlementButton,  canBuild && (isSetupPhase ? !setupSettlementPlaced : CanAffordSettlement(player)));
            SetInteractable(BuildRoadButton,        canBuild && (isSetupPhase ? setupSettlementPlaced  : CanAffordRoad(player)));
            SetInteractable(BuildCityButton,        isBuildPhase && CanAffordCity(player));
            SetInteractable(BuyDevCardButton,       (isBuildPhase || isTradingPhase) && CanAffordDevCard(player));
            SetInteractable(CancelPlacementButton,  manager.CurrentPlacementMode != PlacementMode.None
                                                    && manager.CurrentPlacementMode != PlacementMode.MoveRobber);
        }

        private static bool CanAffordSettlement(CatanPlayer player)
        {
            if (player == null) return false;
            var cost = new GameCore.Resources.ResourceBundle()
                .Add(CatanResources.Wood, 1).Add(CatanResources.Brick, 1)
                .Add(CatanResources.Sheep, 1).Add(CatanResources.Wheat, 1);
            return player.Resources.Current.CanAfford(cost);
        }

        private static bool CanAffordRoad(CatanPlayer player)
        {
            if (player == null) return false;
            var cost = new GameCore.Resources.ResourceBundle()
                .Add(CatanResources.Wood, 1).Add(CatanResources.Brick, 1);
            return player.Resources.Current.CanAfford(cost);
        }

        private static bool CanAffordCity(CatanPlayer player)
        {
            if (player == null) return false;
            var cost = new GameCore.Resources.ResourceBundle()
                .Add(CatanResources.Wheat, 2).Add(CatanResources.Ore, 3);
            return player.Resources.Current.CanAfford(cost);
        }

        private static bool CanAffordDevCard(CatanPlayer player)
        {
            if (player == null) return false;
            var cost = new GameCore.Resources.ResourceBundle()
                .Add(CatanResources.Ore, 1).Add(CatanResources.Wheat, 1).Add(CatanResources.Sheep, 1);
            return player.Resources.Current.CanAfford(cost);
        }

        private static void SetInteractable(Button button, bool interactable)
        {
            if (button != null) 
            {
                button.interactable = interactable;
            }
        }

        private static void SetAllInteractable(bool interactable)
        {
            // No button references here; caller only invokes this when manager is null
        }
    }
}
