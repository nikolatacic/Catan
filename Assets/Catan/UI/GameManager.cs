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

        [Header("Players (2–4 entries)")]
        public List<PlayerConfig> PlayerConfigs = new();

        [Header("Board")]
        public int BoardSeed = 0;
        public BoardRenderer BoardRenderer;
        public RobberView RobberView;
        public StealTargetPanelView StealTargetPanel;

        public CatanBoard Board { get; private set; }
        public List<CatanPlayer> Players { get; private set; } = new();
        public CatanPlayer ActivePlayer => TurnManager.CurrentActor as CatanPlayer;

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

        private CardDeck<DevelopmentCard> _devCardDeck;
        private LargestArmyTracker _largestArmyTracker;
        private LongestRoadTracker _longestRoadTracker;
        private bool _devCardPlayedThisTurn;

        // ── Unity lifecycle ────────────────────────────────────────────────────

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            InitializeResources();
            GenerateBoard();
            CreatePlayers();
            WireSystems();
            SubscribeToEvents();
            _devCardDeck = CreateDevCardDeck();
            BoardRenderer?.RenderBoard();
            RobberView?.SnapToCurrentPosition();
            TurnManager.StartGame();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<KnightPlayedEvent>(OnKnightPlayed);
            EventBus.Unsubscribe<GameCore.Build.BuildSucceededEvent>(OnBuildSucceeded);
            EventBus.Unsubscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Unsubscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
        }

        // ── Initialisation ─────────────────────────────────────────────────────

        private void InitializeResources()
        {
            CatanResources.Initialize(WoodResource, BrickResource, SheepResource, WheatResource, OreResource);
        }

        private void GenerateBoard()
        {
            var random = BoardSeed == 0 ? new System.Random() : new System.Random(BoardSeed);
            var generator = new CatanBoardGenerator(random);
            Board = generator.GenerateBoard();
        }

        private void CreatePlayers()
        {
            Players.Clear();
            foreach (var config in PlayerConfigs)
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

            var victoryCondition = new CatanVictoryCondition(Board);
            ScoreManager.Conditions.Add(victoryCondition);
            ScoreManager.Players.AddRange(allPlayers);

            _largestArmyTracker = new LargestArmyTracker();
            _longestRoadTracker = new LongestRoadTracker();
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<KnightPlayedEvent>(OnKnightPlayed);
            EventBus.Subscribe<GameCore.Build.BuildSucceededEvent>(OnBuildSucceeded);
            EventBus.Subscribe<CatanPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Subscribe<GameCore.Turn.TurnStartedEvent>(OnTurnStarted);
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
            if (TurnManager.CurrentCatanPhase != CatanTurnPhase.RollDice) return;
            TurnManager.RequestRoll();
        }

        public void EndTurn()
        {
            if (TurnManager.CurrentCatanPhase != CatanTurnPhase.Building
                && TurnManager.CurrentCatanPhase != CatanTurnPhase.EndTurn
                && TurnManager.CurrentCatanPhase != CatanTurnPhase.Trading) return;

            CurrentPlacementMode = PlacementMode.None;
            _devCardPlayedThisTurn = false;
            TurnManager.NextTurn();
        }

        public void BeginPlaceSettlement() => CurrentPlacementMode = PlacementMode.Settlement;
        public void BeginPlaceRoad()       => CurrentPlacementMode = PlacementMode.Road;
        public void BeginUpgradeCity()     => CurrentPlacementMode = PlacementMode.City;
        public void CancelPlacement()      => CurrentPlacementMode = PlacementMode.None;

        public void TryPlaceSettlement(GameCore.Board.HexVertex vertex)
        {
            var player = ActivePlayer;
            if (player == null) return;

            bool isSetupPhase = TurnManager.CurrentCatanPhase == CatanTurnPhase.SetupPlacement;

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
                CurrentPlacementMode = PlacementMode.Road;
            }
            else
            {
                player.Resources.TryRemove(settlement.BuildCost);
                CurrentPlacementMode = PlacementMode.None;
            }

            ScoreManager.RecalculateAll();
        }

        public void TryPlaceRoad(GameCore.Board.HexEdge edge)
        {
            var player = ActivePlayer;
            if (player == null) return;

            var road = new Road(player, edge);
            if (!BuildManager.TryPlace(road, road, player)) return;

            Board.Roads[edge] = road;
            player.Roads.Add(road);

            bool isSetupPhase = TurnManager.CurrentCatanPhase == CatanTurnPhase.SetupPlacement;
            if (isSetupPhase)
            {
                SetupSettlementPlaced = false;
                CurrentPlacementMode = PlacementMode.None;
                TurnManager.NextTurn();
            }
            else
            {
                player.Resources.TryRemove(road.BuildCost);
                CurrentPlacementMode = PlacementMode.None;
                _longestRoadTracker.Recalculate(Board);
                ScoreManager.RecalculateAll();
            }
        }

        public void TryUpgradeCity(GameCore.Board.HexVertex vertex)
        {
            var player = ActivePlayer;
            if (player == null) return;
            if (!Board.Settlements.TryGetValue(vertex, out var settlement)) return;
            if (settlement.Owner != player || settlement.IsCity) return;

            settlement.UpgradeToCity();
            if (!BuildManager.TryPlace(settlement, settlement, player))
            {
                // Roll back the upgrade on failure
                settlement.UpgradeToCity(); // not reversible — log warning instead
                Debug.LogWarning("City upgrade failed after UpgradeToCity was called.");
                return;
            }

            player.Resources.TryRemove(settlement.BuildCost);
            ScoreManager.RecalculateAll();
        }

        public void TryMoveRobber(GameCore.Board.HexCoord coord)
        {
            if (TurnManager.CurrentCatanPhase != CatanTurnPhase.Robber) return;

            var eligibleVictims = Board.GetPlayersOnTile(coord)
                .Where(player => player != ActivePlayer)
                .ToList();

            if (eligibleVictims.Count > 1)
            {
                StealTargetPanel?.Show(coord, eligibleVictims);
                return;
            }

            CompleteRobberMove(coord, eligibleVictims.Count == 1 ? eligibleVictims[0] : null);
        }

        public void CompleteRobberMove(GameCore.Board.HexCoord coord, GameCore.Player.IPlayer victim)
        {
            TurnManager.RobberSystem.MoveRobber(coord, ActivePlayer, victim);
            CurrentPlacementMode = PlacementMode.None;
            TurnManager.AdvancePhase();
        }

        public bool TryPurchaseDevCard()
        {
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

            if (card is VictoryPointCard)
                ScoreManager.RecalculateAll();

            return true;
        }

        public bool IsValidSettlementSpot(GameCore.Board.HexVertex vertex)
        {
            var player = ActivePlayer;
            if (player == null) return false;
            var tempSettlement = new Settlement(player, vertex);
            return BuildManager.Rule?.CanPlace(tempSettlement, null, player) ?? false;
        }

        public bool IsValidCitySpot(GameCore.Board.HexVertex vertex)
        {
            if (!Board.Settlements.TryGetValue(vertex, out var settlement)) return false;
            return settlement.Owner == ActivePlayer && !settlement.IsCity;
        }

        public bool IsValidRoadSpot(GameCore.Board.HexEdge edge)
        {
            var player = ActivePlayer;
            if (player == null) return false;
            var tempRoad = new Road(player, edge);
            return BuildManager.Rule?.CanPlace(tempRoad, null, player) ?? false;
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

        public bool TryBankTrade(IResource give, IResource receive)
        {
            var player = ActivePlayer;
            if (player == null) return false;

            int ratio = GetBankTradeRatio(give);
            var offering   = new ResourceBundle().Add(give, ratio);
            var requesting = new ResourceBundle().Add(receive, 1);

            var offer = TradeManager.ProposeTradeToBank(player, offering, requesting);
            if (!TradeManager.AcceptTrade(offer, null)) return false;

            player.Resources.TryRemove(offering);
            player.Resources.TryAdd(requesting);
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
            if (_devCardPlayedThisTurn && card is not VictoryPointCard) return false;

            var context = new CatanGameContext(
                ActivePlayer, TurnManager, Board,
                Players.Cast<GameCore.Player.IPlayer>().ToList());

            if (!card.IsPlayable(context)) return false;

            ActivePlayer.DevelopmentCards.Play(card, context);
            if (card is not VictoryPointCard)
                _devCardPlayedThisTurn = true;

            ScoreManager.RecalculateAll();
            return true;
        }

        // ── Event handlers ─────────────────────────────────────────────────────

        private void OnKnightPlayed(KnightPlayedEvent gameEvent)
        {
            if (gameEvent.Player is CatanPlayer catanPlayer)
                _largestArmyTracker.Update(catanPlayer, catanPlayer.KnightsPlayed);
            ScoreManager.RecalculateAll();
        }

        private void OnBuildSucceeded(GameCore.Build.BuildSucceededEvent gameEvent)
        {
            if (gameEvent.Piece is Road)
                _longestRoadTracker.Recalculate(Board);
            ScoreManager.RecalculateAll();
        }

        private void OnPhaseChanged(CatanPhaseChangedEvent gameEvent)
        {
            if (gameEvent.To == CatanTurnPhase.Robber)
                CurrentPlacementMode = PlacementMode.MoveRobber;
        }

        private void OnTurnStarted(GameCore.Turn.TurnStartedEvent gameEvent)
        {
            _devCardPlayedThisTurn = false;
            SetupSettlementPlaced = false;
            CurrentPlacementMode = PlacementMode.None;
        }
    }
}
