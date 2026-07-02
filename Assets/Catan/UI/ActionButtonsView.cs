using UnityEngine;
using UnityEngine.UIElements;
using Catan.Commands;
using GameCore.Events;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ActionButtonsView : MonoBehaviour
    {
        [Header("Button icons from CatanElements spritesheet (assign in Inspector)")]
        public UnityEngine.Sprite SettlementSprite;
        public UnityEngine.Sprite RoadSprite;
        public UnityEngine.Sprite CitySprite;
        public UnityEngine.Sprite DevCardSprite;

        [Header("Bank trade panel reference")]
        public BankTradePanelView BankTradePanel;

        private Button _rollDiceButton;
        private Button _endTurnButton;
        private Button _buildSettlementButton;
        private Button _buildRoadButton;
        private Button _buildCityButton;
        private Button _buyDevCardButton;
        private Button _cancelPlacementButton;
        private Button _bankTradeButton;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _rollDiceButton        = root.Q<Button>("RollDiceButton");
            _endTurnButton         = root.Q<Button>("EndTurnButton");
            _buildSettlementButton = root.Q<Button>("BuildSettlementButton");
            _buildRoadButton       = root.Q<Button>("BuildRoadButton");
            _buildCityButton       = root.Q<Button>("BuildCityButton");
            _buyDevCardButton      = root.Q<Button>("BuyDevCardButton");
            _cancelPlacementButton = root.Q<Button>("CancelPlacementButton");
            _bankTradeButton       = root.Q<Button>("BankTradeButton");

            _rollDiceButton?.RegisterCallback<ClickEvent>(_ => CommandDispatcher.Send(new RequestRollCommand()));
            _endTurnButton?.RegisterCallback<ClickEvent>(_ => CommandDispatcher.Send(new EndTurnCommand()));
            _buildSettlementButton?.RegisterCallback<ClickEvent>(_ => GameManager.Instance?.BeginPlaceSettlement());
            _buildRoadButton?.RegisterCallback<ClickEvent>(_ => GameManager.Instance?.BeginPlaceRoad());
            _buildCityButton?.RegisterCallback<ClickEvent>(_ => GameManager.Instance?.BeginUpgradeCity());
            _buyDevCardButton?.RegisterCallback<ClickEvent>(_ => CommandDispatcher.Send(new PurchaseDevCardCommand()));
            _cancelPlacementButton?.RegisterCallback<ClickEvent>(_ => GameManager.Instance?.CancelPlacement());
            _bankTradeButton?.RegisterCallback<ClickEvent>(_ => BankTradePanel?.Open());

            ApplyButtonIcons();
        }

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

        // ── Button icon sprites from CatanElements ────────────────────────────

        private void ApplyButtonIcons()
        {
            ApplyIconSprite("SettlementIcon", SettlementSprite);
            ApplyIconSprite("RoadIcon",       RoadSprite);
            ApplyIconSprite("CityIcon",       CitySprite);
            ApplyIconSprite("DevCardIcon",    DevCardSprite);
        }

        private void ApplyIconSprite(string iconElementName, UnityEngine.Sprite sprite)
        {
            if (sprite == null) return;
            var root = GetComponent<UIDocument>().rootVisualElement;
            var iconElement = root.Q<VisualElement>(iconElementName);
            if (iconElement != null)
                iconElement.style.backgroundImage = new StyleBackground(sprite);
        }

        // ── Interactability ────────────────────────────────────────────────────

        private void RefreshButtons()
        {
            var manager = GameManager.Instance;
            if (manager == null || manager.IsGameOver)
            {
                SetAllEnabled(false);
                return;
            }

            // In networked mode, disable all buttons when it is not this device's turn.
            if (Catan.NetworkSession.IsNetworked)
            {
                int localPlayerIndex = Catan.NetworkSession.LocalPlayerIndex;
                if (localPlayerIndex < 0 || manager.Players.Count == 0) return;

                bool isLocalPlayersTurn = localPlayerIndex < manager.Players.Count
                    && manager.ActivePlayer == manager.Players[localPlayerIndex];

                if (!isLocalPlayersTurn)
                {
                    SetAllEnabled(false);
                    return;
                }
            }

            var currentPhase  = manager.TurnManager.CurrentCatanPhase;
            var activePlayer  = manager.ActivePlayer;

            bool isRollPhase    = currentPhase == CatanTurnPhase.RollDice;
            bool isBuildPhase   = currentPhase == CatanTurnPhase.Building;
            bool isTradingPhase = currentPhase == CatanTurnPhase.Trading;
            bool isEndTurnPhase = currentPhase == CatanTurnPhase.EndTurn;
            bool isSetupPhase   = currentPhase == CatanTurnPhase.SetupPlacement;
            bool canBuild       = isBuildPhase || isTradingPhase || isSetupPhase;
            bool canEndTurn     = isBuildPhase || isTradingPhase || isEndTurnPhase;

            bool setupSettlementPlaced = manager.SetupSettlementPlaced;
            bool hasFreeRoads          = manager.FreeRoadsRemaining > 0;

            SetEnabled(_rollDiceButton,        isRollPhase);
            SetEnabled(_endTurnButton,         canEndTurn);
            SetEnabled(_buildSettlementButton, canBuild && (isSetupPhase ? !setupSettlementPlaced : CanAffordSettlement(activePlayer)));
            SetEnabled(_buildRoadButton,       canBuild && (isSetupPhase ? setupSettlementPlaced  : (CanAffordRoad(activePlayer) || hasFreeRoads)));
            SetEnabled(_buildCityButton,       (isBuildPhase || isTradingPhase) && CanAffordCity(activePlayer));
            SetEnabled(_buyDevCardButton,      (isBuildPhase || isTradingPhase) && CanAffordDevCard(activePlayer));
            SetEnabled(_cancelPlacementButton, manager.CurrentPlacementMode != PlacementMode.None
                                               && manager.CurrentPlacementMode != PlacementMode.MoveRobber);
            SetEnabled(_bankTradeButton,       isBuildPhase || isTradingPhase);
        }

        // ── Affordability checks ───────────────────────────────────────────────

        private static bool CanAffordSettlement(CatanPlayer player)
        {
            if (player == null) return false;
            var settlementCost = new GameCore.Resources.ResourceBundle()
                .Add(CatanResources.Wood, 1).Add(CatanResources.Brick, 1)
                .Add(CatanResources.Sheep, 1).Add(CatanResources.Wheat, 1);
            return player.Resources.Current.CanAfford(settlementCost);
        }

        private static bool CanAffordRoad(CatanPlayer player)
        {
            if (player == null) return false;
            var roadCost = new GameCore.Resources.ResourceBundle()
                .Add(CatanResources.Wood, 1).Add(CatanResources.Brick, 1);
            return player.Resources.Current.CanAfford(roadCost);
        }

        private static bool CanAffordCity(CatanPlayer player)
        {
            if (player == null) return false;
            var cityCost = new GameCore.Resources.ResourceBundle()
                .Add(CatanResources.Wheat, 2).Add(CatanResources.Ore, 3);
            return player.Resources.Current.CanAfford(cityCost);
        }

        private static bool CanAffordDevCard(CatanPlayer player)
        {
            if (player == null) return false;
            var devCardCost = new GameCore.Resources.ResourceBundle()
                .Add(CatanResources.Ore, 1).Add(CatanResources.Wheat, 1).Add(CatanResources.Sheep, 1);
            return player.Resources.Current.CanAfford(devCardCost);
        }

        private static void SetEnabled(Button button, bool isEnabled)
        {
            button?.SetEnabled(isEnabled);
        }

        private void SetAllEnabled(bool isEnabled)
        {
            SetEnabled(_rollDiceButton,        isEnabled);
            SetEnabled(_endTurnButton,         isEnabled);
            SetEnabled(_buildSettlementButton, isEnabled);
            SetEnabled(_buildRoadButton,       isEnabled);
            SetEnabled(_buildCityButton,       isEnabled);
            SetEnabled(_buyDevCardButton,      isEnabled);
            SetEnabled(_cancelPlacementButton, isEnabled);
            SetEnabled(_bankTradeButton,       isEnabled);
        }
    }
}
