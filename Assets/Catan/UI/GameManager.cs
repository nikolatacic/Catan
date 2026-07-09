using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameCore.Cards;
using GameCore.Events;
using GameCore.Resources;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Attach to a root GameObject ("GameManager") in the scene.
    // Assign CatanTurnManager, BuildManager, TradeManager, ScoreManager components.
    // Assign five CatanResource ScriptableObjects (Wood/Brick/Sheep/Wheat/Ore).
    // Populate PlayerConfigs with 2–4 player entries before entering Play mode.
    // ──────────────────────────────────────────────────────────────────────────

    public enum PlacementMode { None, Settlement, Road, City, MoveRobber }

    [System.Serializable]
    public class PlayerConfig
    {
        public string PlayerName = "Player";
        public Color PlayerColor = Color.white;
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Systems (add as components on this GameObject or children)")]
        public CatanTurnManager TurnManager;
        public GameCore.Build.BuildManager BuildManager;
        public GameCore.Trade.TradeManager TradeManager;
        public GameCore.Score.ScoreManager ScoreManager;

        [Header("Resource ScriptableObjects")]
        public CatanResource WoodResource;
        public CatanResource BrickResource;
        public CatanResource SheepResource;
        public CatanResource WheatResource;
        public CatanResource OreResource;

        [Header("Players (fallback when GameSession is empty)")]
        [Tooltip("Used only when the scene is opened directly without MainMenu. " +
                 "If GameSession.HasPlayers is true, that list wins.")]
        public List<PlayerConfig> PlayerConfigs = new();

        [Header("Board")]
        public int BoardSeed = 0;
        public BoardRenderer BoardRenderer;
        public RobberView RobberView;
        public StealTargetPanelView StealTargetPanel;

        [Header("Dev card panels")]
        public MonopolyPanelView MonopolyPanel;
        public YearOfPlentyPanelView YearOfPlentyPanel;

        [Header("Victory")]
        [SerializeField] private int _victoryPointsToWin = 10;
        public int VictoryPointsToWin => _victoryPointsToWin;

        public CatanBoard Board { get; private set; }
        public List<CatanPlayer> Players { get; private set; } = new();
        public CatanPlayer ActivePlayer => TurnManager.CurrentActor as CatanPlayer;
        public bool IsGameOver { get; private set; }

        private PlacementMode _currentPlacementMode = PlacementMode.None;
        public PlacementMode CurrentPlacementMode
        {
            get => _currentPlacementMode;
            set
            {
                _currentPlacementMode = value;
                EventBus.Publish(new PlacementModeChangedEvent { Mode = value });
            }
        }

        public bool SetupSettlementPlaced { get; private set; }
        public int FreeRoadsRemaining { get; private set; }

        private CardDeck<DevelopmentCard> _devCardDeck;
        private LargestArmyTracker _largestArmyTracker;
        private LongestRoadTracker _longestRoadTracker;
        private bool _devCardPlayedThisTurn;
        private GameCore.Trade.ITradeOffer _pendingPlayerTradeOffer;

        // ── Unity lifecycle ────────────────────────────────────────────────────

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            InitializeResources();

            if (NetworkSession.IsClient)
            {
                // Defer board generation until the host sends its seed via
                // NetworkEventBridge. Until then the scene is empty of game state.
                return;
            }

            CompleteInitialization(BoardSeed);
        }

        // Called by NetworkEventBridge on the client once the host's seed arrives.
        // Also called from Start() on hotseat / host with the local BoardSeed.
        public void CompleteInitialization(int seed)
        {
            // Convert "0 = random" into a concrete seed so the host can share it
            // with clients via NetworkEventBridge.
            if (seed == 0) seed = new System.Random().Next(1, int.MaxValue);
            BoardSeed = seed;
            Debug.Log($"[GameManager] CompleteInitialization seed={BoardSeed} IsClient={NetworkSession.IsClient}");
            GenerateBoard();
            CreatePlayers();
            WireSystems();
            SubscribeToEvents();
            _devCardDeck = CreateDevCardDeck();
            Debug.Log($"[GameManager] Calling BoardRenderer.RenderBoard — renderer={(BoardRenderer != null ? "OK" : "NULL")}");
            BoardRenderer?.RenderBoard();
            RobberView?.SnapToCurrentPosition();

            // Only the host actually drives the turn machine. Clients receive
            // turn changes via TurnStartedEvent fan-out.
            if (!NetworkSession.IsClient)
                TurnManager.StartGame();

            Debug.Log($"[GameManager] CompleteInitialization done. Players={Players.Count} Board={(Board != null ? "OK" : "NULL")}");
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<KnightPlayedEvent>(OnKnightPlayed);
            EventBus.Unsubscribe<FreeRoadsGrantedEvent>(OnFreeRoadsGranted);
            EventBus.Unsubscribe<GameCore.Build.BuildSucceededEvent>(OnBuildSucceeded);
            EventBus.Unsubscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Unsubscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<GameCore.Score.VictoryAchievedEvent>(OnVictoryAchieved);
        }

        // ── Initialisation ─────────────────────────────────────────────────────

        private void InitializeResources()
        {
            CatanResources.Initialize(WoodResource, BrickResource, SheepResource, WheatResource, OreResource);
        }

        private void GenerateBoard()
        {
            // BoardSeed is always concrete by this point (CompleteInitialization
            // resolves 0 to a random non-zero value).
            var generator = new CatanBoardGenerator(new System.Random(BoardSeed));
            Board = generator.GenerateBoard();
        }

        private void CreatePlayers()
        {
            Players.Clear();

            var configs = GameSession.HasPlayers
                ? (IReadOnlyList<PlayerConfig>)GameSession.Players
                : PlayerConfigs;

            foreach (var config in configs)
            {
                var player = new CatanPlayer(config.PlayerName, config.PlayerName, config.PlayerColor);
                Players.Add(player);
                TurnManager.Actors.Add(player);
            }
        }

        private void WireSystems()
        {
            var allPlayers = Players.Cast<GameCore.Player.IPlayer>().ToList();

            var robberSystem = new RobberSystem();
            robberSystem.Initialize(Board, allPlayers);
            TurnManager.Initialize(Board, null, robberSystem);

            BuildManager.Rule = new CatanBuildRule(Board, TurnManager);
            TradeManager.Rule = new CatanTradeRule(TurnManager, Board.Ports);

            var victoryCondition = new CatanVictoryCondition(Board, _victoryPointsToWin);
            ScoreManager.Conditions.Add(victoryCondition);
            ScoreManager.Players.AddRange(allPlayers);

            _largestArmyTracker = new LargestArmyTracker();
            _longestRoadTracker = new LongestRoadTracker();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<KnightPlayedEvent>(OnKnightPlayed);
            EventBus.Subscribe<FreeRoadsGrantedEvent>(OnFreeRoadsGranted);
            EventBus.Subscribe<GameCore.Build.BuildSucceededEvent>(OnBuildSucceeded);
            EventBus.Subscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Subscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<GameCore.Score.VictoryAchievedEvent>(OnVictoryAchieved);
        }

        private static CardDeck<DevelopmentCard> CreateDevCardDeck()
        {
            var deck = new CardDeck<DevelopmentCard>();
            for (int knightIndex = 0; knightIndex < 14; knightIndex++) deck.AddToBottom(new KnightCard());
            for (int vpIndex = 0; vpIndex < 5; vpIndex++) deck.AddToBottom(new VictoryPointCard());
            for (int rbIndex = 0; rbIndex < 2; rbIndex++) deck.AddToBottom(new RoadBuildingCard());
            for (int ypIndex = 0; ypIndex < 2; ypIndex++) deck.AddToBottom(new YearOfPlentyCard());
            for (int mIndex = 0; mIndex < 2; mIndex++) deck.AddToBottom(new MonopolyCard());
            deck.Shuffle();
            return deck;
        }

        // ── Game actions (called by view components) ───────────────────────────

        public void RequestRoll()
        {
            if (IsGameOver) return;
            if (TurnManager.CurrentCatanPhase != CatanTurnPhase.RollDice) return;
            TurnManager.RequestRoll();
        }

        public void EndTurn()
        {
            if (IsGameOver) return;
            if (TurnManager.CurrentCatanPhase != CatanTurnPhase.Building
                && TurnManager.CurrentCatanPhase != CatanTurnPhase.EndTurn
                && TurnManager.CurrentCatanPhase != CatanTurnPhase.Trading) return;

            CurrentPlacementMode = PlacementMode.None;
            _devCardPlayedThisTurn = false;
            TurnManager.NextTurn();
        }

        public void BeginPlaceSettlement() { if (!IsGameOver) CurrentPlacementMode = PlacementMode.Settlement; }
        public void BeginPlaceRoad()       { if (!IsGameOver) CurrentPlacementMode = PlacementMode.Road; }
        public void BeginUpgradeCity()     { if (!IsGameOver) CurrentPlacementMode = PlacementMode.City; }
        public void CancelPlacement()      => CurrentPlacementMode = PlacementMode.None;

        public void TryPlaceSettlement(GameCore.Board.HexVertex vertex)
        {
            if (IsGameOver) return;
            var player = ActivePlayer;
            if (player == null) return;

            bool isSetupPhase = TurnManager.CurrentCatanPhase == CatanTurnPhase.SetupPlacement;

            // Reject a second settlement attempt in the same setup turn.
            if (isSetupPhase && SetupSettlementPlaced) return;

            // Set flag BEFORE TryPlace so BuildSucceededEvent sees correct state when
            // ActionButtonsView.RefreshButtons() fires synchronously inside TryPlace.
            if (isSetupPhase) SetupSettlementPlaced = true;

            var settlement = new Settlement(player, vertex);
            if (!BuildManager.TryPlace(settlement, settlement, player))
            {
                if (isSetupPhase) SetupSettlementPlaced = false;
                return;
            }

            Board.Settlements[vertex] = settlement;
            player.Settlements.Add(settlement);

            if (isSetupPhase)
            {
                if (TurnManager.IsSecondSetupRound)
                    GrantAdjacentResources(player, vertex);
                // Only change placement mode on the device whose player just acted.
                // Clients manage their own mode via OnBuildSucceeded; the host must
                // not overwrite its mode when processing another player's command.
                if (IsLocalPlayerAction(player))
                    CurrentPlacementMode = PlacementMode.Road;
            }
            else
            {
                player.Resources.TryRemove(settlement.BuildCost);
                if (IsLocalPlayerAction(player))
                    CurrentPlacementMode = PlacementMode.None;
            }

            RecalculateScoresAndCheckVictory();
        }

        public void TryPlaceRoad(GameCore.Board.HexEdge edge)
        {
            if (IsGameOver) return;
            var player = ActivePlayer;
            if (player == null) return;

            bool isSetupPhase = TurnManager.CurrentCatanPhase == CatanTurnPhase.SetupPlacement;
            bool isFreeRoad   = !isSetupPhase && FreeRoadsRemaining > 0;

            var road = new Road(player, edge);

            // Free roads bypass the resource check inside CatanBuildRule by temporarily
            // granting the cost so the rule sees the player as able to afford it.
            if (isFreeRoad)
                player.Resources.TryAdd(road.BuildCost);

            if (!BuildManager.TryPlace(road, road, player))
            {
                if (isFreeRoad)
                    player.Resources.TryRemove(road.BuildCost);
                return;
            }

            Board.Roads[edge] = road;
            player.Roads.Add(road);

            if (isSetupPhase)
            {
                SetupSettlementPlaced = false;
                if (IsLocalPlayerAction(player))
                    CurrentPlacementMode = PlacementMode.None;
                TurnManager.NextTurn();
            }
            else if (isFreeRoad)
            {
                player.Resources.TryRemove(road.BuildCost); // cancel the temp grant
                FreeRoadsRemaining--;
                if (IsLocalPlayerAction(player))
                    CurrentPlacementMode = FreeRoadsRemaining > 0 ? PlacementMode.Road : PlacementMode.None;
                _longestRoadTracker.Recalculate(Board);
                RecalculateScoresAndCheckVictory();
            }
            else
            {
                player.Resources.TryRemove(road.BuildCost);
                if (IsLocalPlayerAction(player))
                    CurrentPlacementMode = PlacementMode.None;
                _longestRoadTracker.Recalculate(Board);
                RecalculateScoresAndCheckVictory();
            }
        }

        public void TryUpgradeCity(GameCore.Board.HexVertex vertex)
        {
            if (IsGameOver) return;
            var player = ActivePlayer;
            if (player == null) return;
            if (!Board.Settlements.TryGetValue(vertex, out var existingSettlement)) return;

            var upgrade = new CityUpgrade(player, vertex);
            if (!BuildManager.TryPlace(upgrade, upgrade, player)) return;

            existingSettlement.UpgradeToCity();
            player.Resources.TryRemove(upgrade.BuildCost);
            RecalculateScoresAndCheckVictory();
        }

        public void TryMoveRobber(GameCore.Board.HexCoord coord)
        {
            if (IsGameOver) return;
            if (TurnManager.CurrentCatanPhase != CatanTurnPhase.Robber) return;

            var eligibleVictims = Board.GetPlayersOnTile(coord)
                .Where(player => player != ActivePlayer)
                .ToList();

            if (eligibleVictims.Count > 0)
            {
                if (StealTargetPanel != null)
                {
                    StealTargetPanel.Show(coord, eligibleVictims);
                    return;
                }
                // Panel not wired in Inspector — auto-steal from the first/only victim.
                CompleteRobberMove(coord, eligibleVictims[0]);
                return;
            }

            CompleteRobberMove(coord, null);
        }

        public void CompleteRobberMove(GameCore.Board.HexCoord coord, GameCore.Player.IPlayer victim)
        {
            if (IsGameOver) return;
            TurnManager.RobberSystem.MoveRobber(coord, ActivePlayer, victim);
            CurrentPlacementMode = PlacementMode.None;
            TurnManager.AdvancePhase();
        }

        public bool TryPurchaseDevCard()
        {
            if (IsGameOver) return false;
            var player = ActivePlayer;
            if (player == null || _devCardDeck.Count == 0) return false;
            if (TurnManager.CurrentCatanPhase != CatanTurnPhase.Building
                && TurnManager.CurrentCatanPhase != CatanTurnPhase.Trading) return false;

            var cost = new ResourceBundle()
                .Add(CatanResources.Ore, 1)
                .Add(CatanResources.Wheat, 1)
                .Add(CatanResources.Sheep, 1);

            if (!player.Resources.TryRemove(cost)) return false;

            var card = _devCardDeck.Draw();
            if (card == null) return false;

            card.TurnPurchased = TurnManager.TurnNumber;
            player.DevelopmentCards.Add(card);

            EventBus.Publish(new DevCardPurchasedEvent { Player = player });

            RecalculateScoresAndCheckVictory();
            return true;
        }

        private void GrantAdjacentResources(CatanPlayer player, GameCore.Board.HexVertex vertex)
        {
            foreach (var tileCoord in vertex.AdjacentTiles)
            {
                if (!Board.Grid.Tiles.TryGetValue(tileCoord, out var catanTile)) continue;
                if (catanTile.Resource == null) continue;
                player.Resources.TryAdd(new ResourceBundle().Add(catanTile.Resource, 1));
            }
        }

        public int GetBankTradeRatio(IResource resource)
        {
            var player = ActivePlayer;
            if (player == null) return 4;
            var resourceType = ResourceToType(resource);
            return resourceType.HasValue ? Board.Ports.GetTradeRatio(player, resourceType.Value) : 4;
        }

        // ── Player-to-player trading ───────────────────────────────────────────

        public bool TryProposePlayerTrade(int targetPlayerIndex, ResourceBundle offering, ResourceBundle requesting)
        {
            if (IsGameOver) return false;
            var proposer = ActivePlayer;
            if (proposer == null) return false;

            var phase = TurnManager.CurrentCatanPhase;
            if (phase != CatanTurnPhase.Trading && phase != CatanTurnPhase.Building) return false;

            if (targetPlayerIndex < 0 || targetPlayerIndex >= Players.Count) return false;
            var target = Players[targetPlayerIndex];
            if (target == proposer) return false;

            if (!proposer.Resources.CanAfford(offering)) return false;

            bool offeringHasResources = false;
            bool requestingHasResources = false;
            foreach (var resource in CatanResources.All)
            {
                if (offering.Get(resource) > 0) offeringHasResources = true;
                if (requesting.Get(resource) > 0) requestingHasResources = true;
            }
            if (!offeringHasResources || !requestingHasResources) return false;

            if (_pendingPlayerTradeOffer != null)
            {
                TradeManager.CancelTrade(_pendingPlayerTradeOffer);
                _pendingPlayerTradeOffer = null;
            }

            _pendingPlayerTradeOffer = TradeManager.ProposeTradeToPlayer(proposer, target, offering, requesting);
            return true;
        }

        public bool TryAcceptPlayerTrade(int acceptingPlayerIndex)
        {
            if (IsGameOver || _pendingPlayerTradeOffer == null) return false;
            if (acceptingPlayerIndex < 0 || acceptingPlayerIndex >= Players.Count) return false;

            var responder = Players[acceptingPlayerIndex];
            var offer = _pendingPlayerTradeOffer;
            _pendingPlayerTradeOffer = null;

            if (!TradeManager.AcceptTrade(offer, responder)) return false;

            (offer.Proposer as CatanPlayer)?.Resources.TryRemove(offer.Offering);
            (offer.Proposer as CatanPlayer)?.Resources.TryAdd(offer.Requesting);
            (responder as CatanPlayer)?.Resources.TryRemove(offer.Requesting);
            (responder as CatanPlayer)?.Resources.TryAdd(offer.Offering);

            RecalculateScoresAndCheckVictory();
            return true;
        }

        public void TryDeclinePlayerTrade(int decliningPlayerIndex)
        {
            if (_pendingPlayerTradeOffer == null) return;
            if (decliningPlayerIndex < 0 || decliningPlayerIndex >= Players.Count) return;

            var offer = _pendingPlayerTradeOffer;
            _pendingPlayerTradeOffer = null;
            TradeManager.RejectTrade(offer, Players[decliningPlayerIndex]);
        }

        public bool TryCounterPlayerTrade(int counteringPlayerIndex, ResourceBundle counterOffering, ResourceBundle counterRequesting)
        {
            if (IsGameOver || _pendingPlayerTradeOffer == null) return false;
            if (counteringPlayerIndex < 0 || counteringPlayerIndex >= Players.Count) return false;

            var originalProposer = _pendingPlayerTradeOffer.Proposer;
            var counteringPlayer = Players[counteringPlayerIndex];

            var originalOffer = _pendingPlayerTradeOffer;
            _pendingPlayerTradeOffer = null;
            TradeManager.CancelTrade(originalOffer);

            _pendingPlayerTradeOffer = TradeManager.ProposeTradeToPlayer(
                counteringPlayer, originalProposer, counterOffering, counterRequesting);
            return true;
        }

        public void TryCancelPlayerTrade()
        {
            if (_pendingPlayerTradeOffer == null) return;
            var offer = _pendingPlayerTradeOffer;
            _pendingPlayerTradeOffer = null;
            TradeManager.CancelTrade(offer);
        }

        public bool TryBankTrade(IResource give, IResource receive)
        {
            if (IsGameOver) return false;
            var player = ActivePlayer;
            if (player == null) return false;

            int ratio = GetBankTradeRatio(give);
            var offering   = new ResourceBundle().Add(give, ratio);
            var requesting = new ResourceBundle().Add(receive, 1);

            var offer = TradeManager.ProposeTradeToBank(player, offering, requesting);
            if (!TradeManager.AcceptTrade(offer, null)) return false;

            player.Resources.TryRemove(offering);
            player.Resources.TryAdd(requesting);
            RecalculateScoresAndCheckVictory();
            return true;
        }

        private static CatanResourceType? ResourceToType(IResource resource)
        {
            if (resource == CatanResources.Wood)  return CatanResourceType.Wood;
            if (resource == CatanResources.Brick) return CatanResourceType.Brick;
            if (resource == CatanResources.Sheep) return CatanResourceType.Sheep;
            if (resource == CatanResources.Wheat) return CatanResourceType.Wheat;
            if (resource == CatanResources.Ore)   return CatanResourceType.Ore;
            return null;
        }

        public bool TryPlayDevCard(DevelopmentCard card)
        {
            if (IsGameOver) return false;
            if (_devCardPlayedThisTurn && card is not VictoryPointCard) return false;

            var context = new CatanGameContext(
                ActivePlayer, TurnManager, Board,
                Players.Cast<GameCore.Player.IPlayer>().ToList());

            if (!card.IsPlayable(context)) return false;

            // Monopoly and Year of Plenty need the player to pick resources first.
            // Open the modal and defer execution — CompleteDevCardExecution is called back
            // when the player confirms (or the modal is cancelled with no effect).
            if (card is MonopolyCard monopolyCard)
            {
                MonopolyPanel?.Open(monopolyCard, context);
                return true;
            }

            if (card is YearOfPlentyCard yearOfPlentyCard)
            {
                YearOfPlentyPanel?.Open(yearOfPlentyCard, context);
                return true;
            }

            CompleteDevCardExecution(card, context);
            return true;
        }

        // Called directly for Knight / RoadBuilding / VictoryPoint, and by the
        // Monopoly / YearOfPlenty modals after the player has made their selection.
        internal void CompleteDevCardExecution(DevelopmentCard card, CatanGameContext context)
        {
            ActivePlayer.DevelopmentCards.Play(card, context);
            if (card is not VictoryPointCard)
                _devCardPlayedThisTurn = true;
            RecalculateScoresAndCheckVictory();
        }

        // ── Event handlers ─────────────────────────────────────────────────────

        private void OnKnightPlayed(KnightPlayedEvent gameEvent)
        {
            if (gameEvent.Player is CatanPlayer catanPlayer)
                _largestArmyTracker.Update(catanPlayer, catanPlayer.KnightsPlayed);
            RecalculateScoresAndCheckVictory();
        }

        private void OnFreeRoadsGranted(FreeRoadsGrantedEvent gameEvent)
        {
            FreeRoadsRemaining += gameEvent.Count;
            if (IsLocalPlayerAction(gameEvent.Player as CatanPlayer))
                BeginPlaceRoad();
            RecalculateScoresAndCheckVictory();
        }

        private void OnBuildSucceeded(GameCore.Build.BuildSucceededEvent gameEvent)
        {
            if (gameEvent.Piece is Road)
                _longestRoadTracker.Recalculate(Board);

            // In multiplayer the client never runs TryPlace* directly, so the
            // placement-mode transitions that happen inside those methods don't fire.
            // Mirror them here when our own piece is confirmed by the host.
            if (NetworkSession.IsClient)
                ApplyClientSetupModeTransition(gameEvent);

            RecalculateScoresAndCheckVictory();
        }

        private void ApplyClientSetupModeTransition(GameCore.Build.BuildSucceededEvent gameEvent)
        {
            if (TurnManager.CurrentCatanPhase != CatanTurnPhase.SetupPlacement) return;
            int localIndex = NetworkSession.LocalPlayerIndex;
            if (localIndex < 0 || localIndex >= Players.Count) return;
            if (gameEvent.Player != Players[localIndex]) return;

            if (gameEvent.Piece is Settlement)
            {
                SetupSettlementPlaced = true;
                CurrentPlacementMode = PlacementMode.Road;
            }
            else if (gameEvent.Piece is Road)
            {
                SetupSettlementPlaced = false;
                CurrentPlacementMode = PlacementMode.None;
            }
        }

        // True when the acting player is the one sitting at this device.
        // In hotseat every action is local; in multiplayer only matching index actions are.
        private bool IsLocalPlayerAction(CatanPlayer player)
        {
            if (!NetworkSession.IsNetworked) return true;
            int localIndex = NetworkSession.LocalPlayerIndex;
            return localIndex >= 0 && localIndex < Players.Count && Players[localIndex] == player;
        }

        private void OnVictoryAchieved(GameCore.Score.VictoryAchievedEvent gameEvent)
        {
            IsGameOver = true;
        }

        private void RecalculateScoresAndCheckVictory()
        {
            ScoreManager.RecalculateAll();
            if (!IsGameOver)
                ScoreManager.CheckVictory();

            EventBus.Publish(new GameStateChangedEvent());
        }

        private void OnPhaseChanged(CatanPhaseChangedEvent gameEvent)
        {
            if (gameEvent.To == CatanTurnPhase.Robber)
                CurrentPlacementMode = PlacementMode.MoveRobber;
        }

        private void OnTurnStarted(GameCore.Turn.TurnStartedEvent gameEvent)
        {
            _devCardPlayedThisTurn = false;
            SetupSettlementPlaced  = false;
            FreeRoadsRemaining     = 0;
            CurrentPlacementMode   = PlacementMode.None;

            if (_pendingPlayerTradeOffer != null)
            {
                TradeManager.CancelTrade(_pendingPlayerTradeOffer);
                _pendingPlayerTradeOffer = null;
            }
        }
    }
}
