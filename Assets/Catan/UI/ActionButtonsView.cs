using UnityEngine;
using UnityEngine.UI;
using Catan.Commands;
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

        [Header("Bank trade")]
        public Button BankTradeButton;
        public BankTradePanelView BankTradePanel;

        private void OnEnable()
        {
            EventBus.Subscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Subscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<GameCore.Build.BuildSucceededEvent>(OnBuildSucceeded);
            EventBus.Subscribe<GameCore.Score.VictoryAchievedEvent>(OnVictoryAchieved);
            EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Subscribe<LocalPlayerAssignedEvent>(OnLocalPlayerAssigned);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Unsubscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<GameCore.Build.BuildSucceededEvent>(OnBuildSucceeded);
            EventBus.Unsubscribe<GameCore.Score.VictoryAchievedEvent>(OnVictoryAchieved);
            EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Unsubscribe<LocalPlayerAssignedEvent>(OnLocalPlayerAssigned);
        }

        private void Start() => RefreshButtons();

        private void OnPhaseChanged(CatanPhaseChangedEvent gameEvent) => RefreshButtons();
        private void OnTurnStarted(GameCore.Turn.TurnStartedEvent gameEvent) => RefreshButtons();
        private void OnBuildSucceeded(GameCore.Build.BuildSucceededEvent gameEvent) => RefreshButtons();
        private void OnVictoryAchieved(GameCore.Score.VictoryAchievedEvent gameEvent) => RefreshButtons();
        private void OnGameStateChanged(GameStateChangedEvent gameEvent) => RefreshButtons();
        private void OnLocalPlayerAssigned(LocalPlayerAssignedEvent gameEvent) => RefreshButtons();

        // ── Button callbacks ───────────────────────────────────────────────────

        // Local-only UI-mode toggles (no network — see NetworkBoundaryAudit.md)
        public void OnBuildSettlement() => GameManager.Instance?.BeginPlaceSettlement();
        public void OnBuildRoad()       => GameManager.Instance?.BeginPlaceRoad();
        public void OnBuildCity()       => GameManager.Instance?.BeginUpgradeCity();
        public void OnCancelPlacement() => GameManager.Instance?.CancelPlacement();

        // Player intents that go through the command dispatcher
        public void OnRollDice()   => CommandDispatcher.Send(new RequestRollCommand());
        public void OnEndTurn()    => CommandDispatcher.Send(new EndTurnCommand());
        public void OnBuyDevCard() => CommandDispatcher.Send(new PurchaseDevCardCommand());

        public void OnBankTrade() => BankTradePanel?.Open();

        // ── Interactability ────────────────────────────────────────────────────

        private void RefreshButtons()
        {
            var manager = GameManager.Instance;
            if (manager == null || manager.IsGameOver)
            {
                SetAllInteractable(false);
                return;
            }

            // Networked: only the device whose local player is also the active
            // player gets functional buttons. Other peers see disabled buttons
            // until it's their turn.
            if (Catan.NetworkSession.IsNetworked)
            {
                int localIndex = Catan.NetworkSession.LocalPlayerIndex;
                // If local index not yet assigned, or players haven't been created
                // yet, wait — a TurnStartedEvent or LocalPlayerAssignedEvent will
                // re-trigger this after initialization completes.
                if (localIndex < 0 || manager.Players.Count == 0)
                    return;

                if (localIndex >= manager.Players.Count ||
                    manager.ActivePlayer != manager.Players[localIndex])
                {
                    SetAllInteractable(false);
                    return;
                }
            }

            var phase = manager.TurnManager.CurrentCatanPhase;
            var player = manager.ActivePlayer;

            bool isRollPhase     = phase == CatanTurnPhase.RollDice;
            bool isBuildPhase    = phase == CatanTurnPhase.Building;
            bool isTradingPhase  = phase == CatanTurnPhase.Trading;
            bool isEndTurnPhase  = phase == CatanTurnPhase.EndTurn;
            bool isSetupPhase    = phase == CatanTurnPhase.SetupPlacement;
            bool canBuild        = isBuildPhase || isTradingPhase || isSetupPhase;
            bool canEndTurn      = isBuildPhase || isTradingPhase || isEndTurnPhase;

            SetInteractable(RollDiceButton,         isRollPhase);
            SetInteractable(EndTurnButton,          canEndTurn);
            bool setupSettlementPlaced = manager.SetupSettlementPlaced;
            SetInteractable(BuildSettlementButton,  canBuild && (isSetupPhase ? !setupSettlementPlaced : CanAffordSettlement(player)));
            SetInteractable(BuildRoadButton,        canBuild && (isSetupPhase ? setupSettlementPlaced  : CanAffordRoad(player)));
            SetInteractable(BuildCityButton,        (isBuildPhase || isTradingPhase) && CanAffordCity(player));
            SetInteractable(BuyDevCardButton,       (isBuildPhase || isTradingPhase) && CanAffordDevCard(player));
            SetInteractable(CancelPlacementButton,  manager.CurrentPlacementMode != PlacementMode.None
                                                    && manager.CurrentPlacementMode != PlacementMode.MoveRobber);
            SetInteractable(BankTradeButton,        isBuildPhase || isTradingPhase);
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

        private void SetAllInteractable(bool interactable)
        {
            SetInteractable(RollDiceButton, interactable);
            SetInteractable(EndTurnButton, interactable);
            SetInteractable(BuildSettlementButton, interactable);
            SetInteractable(BuildRoadButton, interactable);
            SetInteractable(BuildCityButton, interactable);
            SetInteractable(BuyDevCardButton, interactable);
            SetInteractable(CancelPlacementButton, interactable);
            SetInteractable(BankTradeButton, interactable);
        }
    }
}
