# Catan Unity — Architecture Plan

## Design Principles

- **Event-first**: systems communicate via `EventBus.Publish<T>()` — no direct references between systems
- **Interface-driven**: every system exposes an interface; concrete types live in the implementation layer
- **Package = boundary**: each package compiles independently and ships without knowing about Catan
- **No MonoBehaviour in logic**: core classes are plain C#; MonoBehaviour adapters live in a thin View/Runner layer

---

## Package Structure

```
Packages/
  com.gamecore.events/          # EventBus, ScriptableObject channels
  com.gamecore.statemachine/    # Generic state machine
  com.gamecore.turn/            # Turn & phase management
  com.gamecore.board/           # HexGrid, coordinates, vertices, edges
  com.gamecore.player/          # IPlayer contract, PlayerManager
  com.gamecore.resources/       # IResource, ResourceBundle, IResourceInventory
  com.gamecore.cards/           # ICard, CardDeck<T>, CardHand<T>
  com.gamecore.trade/           # ITradeOffer, TradeManager, ITradeRule
  com.gamecore.build/           # IPlaceable, IBuildRule, BuildManager
  com.gamecore.score/           # IVictoryCondition, ScoreManager

Assets/Catan/
  Board/                        # CatanHexTile, CatanBoard, PortSystem
  Players/                      # CatanPlayer
  Turn/                         # CatanTurnManager, DiceManager, RobberSystem
  Cards/                        # DevelopmentCard subtypes
  Rules/                        # CatanBuildRule, CatanTradeRule
  Score/                        # CatanVictoryCondition, LargestArmyTracker, LongestRoadTracker
  UI/                           # View layer — subscribes to events, renders state
  ScriptableObjects/            # EventChannels, PlayerData, ResourceDefinitions
```

---

## Infrastructure Packages

### `com.gamecore.events`

The foundation every other package depends on. Two delivery modes coexist:

**Static EventBus** — for pure C# runtime code
```csharp
public interface IGameEvent { }

public static class EventBus
{
    public static void Subscribe<T>(Action<T> handler) where T : IGameEvent;
    public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent;
    public static void Publish<T>(T evt) where T : IGameEvent;
}
```

**ScriptableObject EventChannel** — for inspector-wired Unity scenes
```csharp
public abstract class GameEventChannel<T> : ScriptableObject
{
    public void Raise(T value);
    public void Register(UnityAction<T> listener);
    public void Unregister(UnityAction<T> listener);
}

public interface IEventListener<T>
{
    void OnEventRaised(T value);
}
```

**Pattern**: game logic publishes via `EventBus.Publish<T>()`. UI MonoBehaviours either subscribe directly to EventBus or get wired via EventChannels in the Inspector.

---

### `com.gamecore.statemachine`

Generic state machine used by `TurnSystem`, `RobberSystem`, and UI flows.

```csharp
public interface IState<TContext>
{
    void Enter(TContext context);
    void Execute(TContext context);
    void Exit(TContext context);
}

public class StateMachine<TContext>
{
    public IState<TContext> CurrentState { get; private set; }
    public void Transition(IState<TContext> newState);
    public void Update();
}

public class StateTransition<TContext>
{
    public IState<TContext> From;
    public IState<TContext> To;
    public Func<bool> Condition;
}
```

Events: `StateEnteredEvent<T>`, `StateExitedEvent<T>`, `TransitionFiredEvent<T>`

---

## Core Game System Packages

### `com.gamecore.turn`

Orchestrates whose turn it is and what phase is active. Other systems react to phase events rather than being called directly.

```csharp
public interface ITurnActor
{
    string Id { get; }
    bool CanAct { get; }
}

public class TurnManager : MonoBehaviour
{
    public List<ITurnActor> Actors { get; }
    public ITurnActor CurrentActor { get; private set; }
    public TurnPhase CurrentPhase { get; private set; }
    public int TurnNumber { get; private set; }

    public void NextTurn();
    public void AdvancePhase();
    public void SkipActor(ITurnActor actor, string reason);
}

public enum TurnPhase { Start, Main, End }   // overridden in Catan
```

Events:
```csharp
TurnStartedEvent    { ITurnActor Actor; int TurnNumber; }
TurnEndedEvent      { ITurnActor Actor; }
PhaseChangedEvent   { TurnPhase From; TurnPhase To; }
ActorSkippedEvent   { ITurnActor Actor; string Reason; }
```

---

### `com.gamecore.board`

Pure data — no rendering. Provides hex math, grid storage, and the topological graph of tiles/vertices/edges.

```csharp
public struct HexCoord
{
    public int Q, R;                          // axial coordinates
    public static HexCoord[] Directions;      // 6 unit directions
    public HexCoord[] Neighbors();
    public int Distance(HexCoord other);
    public Vector3 ToWorldPosition(float size);
    public static HexCoord FromWorldPosition(Vector3 pos, float size);
}

public interface IHexTile
{
    HexCoord Coord { get; }
}

public class HexVertex     // shared intersection between 3 tiles
{
    public HexCoord[] AdjacentTiles;      // 2–3 tiles
    public HexEdge[] AdjacentEdges;       // 2–3 edges
}

public class HexEdge       // shared edge between 2 tiles
{
    public HexCoord[] AdjacentTiles;      // 1–2 tiles
    public HexVertex[] AdjacentVertices;  // 2 vertices
}

public class HexGrid<T> where T : IHexTile
{
    public Dictionary<HexCoord, T> Tiles { get; }
    public T GetTile(HexCoord coord);
    public List<T> GetNeighbors(HexCoord coord);
    public List<HexVertex> GetVertices(HexCoord coord);
    public List<HexEdge> GetEdges(HexCoord coord);
}

public abstract class BoardGenerator<T> where T : IHexTile
{
    public abstract HexGrid<T> Generate();
}
```

Events: `TileSelectedEvent { HexCoord }`, `VertexSelectedEvent { HexVertex }`, `EdgeSelectedEvent { HexEdge }`

---

### `com.gamecore.player`

Minimal player contract. Games extend `IPlayer` with game-specific state.

```csharp
public interface IPlayer
{
    string Id { get; }
    string DisplayName { get; }
    Color Color { get; }
}

[CreateAssetMenu]
public class PlayerData : ScriptableObject, IPlayer { ... }

public class PlayerManager : MonoBehaviour
{
    public IReadOnlyList<IPlayer> Players { get; }
    public IPlayer GetPlayer(string id);
    public void AddPlayer(IPlayer player);
    public void RemovePlayer(string id);
}
```

Events: `PlayerJoinedEvent { IPlayer }`, `PlayerLeftEvent { IPlayer }`, `ActivePlayerChangedEvent { IPlayer Prev; IPlayer Next; }`

---

## Game Mechanics Packages

### `com.gamecore.resources`

Type-agnostic resource economy. Games define their own `IResource` implementations.

```csharp
public interface IResource
{
    string ResourceId { get; }
    string DisplayName { get; }
}

public class ResourceBundle        // immutable value object
{
    private readonly Dictionary<IResource, int> _amounts;
    public int Get(IResource res);
    public ResourceBundle Add(IResource res, int amount);
    public ResourceBundle Remove(IResource res, int amount);
    public bool CanAfford(ResourceBundle cost);
    public static ResourceBundle operator +(ResourceBundle a, ResourceBundle b);
}

public interface IResourceInventory
{
    ResourceBundle Current { get; }
    bool TryAdd(ResourceBundle bundle);
    bool TryRemove(ResourceBundle bundle);
    bool CanAfford(ResourceBundle cost);
}

public class ResourceInventory : IResourceInventory { ... }
```

Events:
```csharp
ResourceAddedEvent      { IPlayer Player; ResourceBundle Added; }
ResourceRemovedEvent    { IPlayer Player; ResourceBundle Removed; }
ResourceInsufficientEvent { IPlayer Player; ResourceBundle Needed; }
ResourceTransferredEvent  { IPlayer From; IPlayer To; ResourceBundle Bundle; }
```

---

### `com.gamecore.cards`

Generic deck/hand system. `ICard.OnPlay` accepts a context so cards are self-contained.

```csharp
public interface ICard
{
    string CardId { get; }
    string DisplayName { get; }
    bool IsPlayable(IGameContext context);
    void OnPlay(IGameContext context);
}

public class CardDeck<T> where T : ICard
{
    public int Count { get; }
    public T Draw();
    public void Shuffle();
    public void AddToBottom(T card);
    public void AddToTop(T card);
}

public class CardHand<T> where T : ICard
{
    public IReadOnlyList<T> Cards { get; }
    public void Add(T card);
    public void Remove(T card);
    public void Play(T card, IGameContext context);  // calls card.OnPlay, fires event
}
```

Events: `CardDrawnEvent<T>`, `CardPlayedEvent<T>`, `CardDiscardedEvent<T>`, `DeckEmptyEvent`

---

### `com.gamecore.trade`

Negotiation loop between players (or bank). `ITradeRule` validates legality.

```csharp
public enum TradeStatus { Pending, Accepted, Rejected, Countered, Cancelled }

public interface ITradeOffer
{
    string OfferId { get; }
    IPlayer Proposer { get; }
    IPlayer? Target { get; }       // null = bank
    ResourceBundle Offering { get; }
    ResourceBundle Requesting { get; }
    TradeStatus Status { get; }
}

public interface ITradeRule
{
    bool CanTrade(ITradeOffer offer, IPlayer responder);
    string GetRejectionReason(ITradeOffer offer, IPlayer responder);
}

public class TradeManager : MonoBehaviour
{
    public ITradeRule Rule { get; set; }       // inject Catan's rule

    public ITradeOffer ProposeTradeToPlayer(IPlayer proposer, IPlayer target,
                                             ResourceBundle offering, ResourceBundle requesting);
    public ITradeOffer ProposeTradeToBank(IPlayer proposer,
                                          ResourceBundle offering, ResourceBundle requesting);
    public bool AcceptTrade(ITradeOffer offer, IPlayer responder);
    public bool RejectTrade(ITradeOffer offer, IPlayer responder);
    public void CancelTrade(ITradeOffer offer);
}
```

Events: `TradeProposedEvent`, `TradeAcceptedEvent`, `TradeRejectedEvent`, `TradeCompletedEvent`, `TradeCancelledEvent`

---

### `com.gamecore.build`

Placement/removal of game pieces on board locations. `IBuildRule` validates legality.

```csharp
public interface IPlaceable
{
    string PlaceableId { get; }
    ResourceBundle BuildCost { get; }
}

public interface IBuildLocation
{
    string LocationId { get; }
    bool IsOccupied { get; }
    IPlaceable OccupiedBy { get; }
}

public interface IBuildRule
{
    bool CanPlace(IPlaceable piece, IBuildLocation location, IPlayer player);
    bool CanRemove(IPlaceable piece, IBuildLocation location, IPlayer player);
    string GetFailureReason(IPlaceable piece, IBuildLocation location, IPlayer player);
}

public class BuildManager : MonoBehaviour
{
    public IBuildRule Rule { get; set; }

    public bool TryPlace(IPlaceable piece, IBuildLocation location, IPlayer player);
    public bool TryRemove(IPlaceable piece, IBuildLocation location);
}
```

Events: `BuildAttemptedEvent`, `BuildSucceededEvent { IPlayer, IPlaceable, IBuildLocation }`, `BuildFailedEvent { IPlayer, string Reason }`, `PieceRemovedEvent`

---

### `com.gamecore.score`

Pluggable victory condition evaluation. Multiple `IVictoryCondition` instances can contribute.

```csharp
public interface IVictoryCondition
{
    string Name { get; }
    int CalculatePoints(IPlayer player);
    bool IsWinCondition(IPlayer player);    // true = game over
}

public class ScoreManager : MonoBehaviour
{
    public List<IVictoryCondition> Conditions { get; }

    public int GetScore(IPlayer player);
    public void RecalculateAll();
    public IPlayer GetLeader();
    public IPlayer? CheckVictory();         // returns winner or null
}
```

Events: `ScoreChangedEvent { IPlayer, int OldScore, int NewScore }`, `LeaderChangedEvent`, `VictoryAchievedEvent { IPlayer Winner }`

---

## Catan Implementation Layer

### Resource & Tile

```csharp
public enum CatanResourceType { Wood, Brick, Sheep, Wheat, Ore }

[CreateAssetMenu]
public class CatanResource : ScriptableObject, IResource
{
    public CatanResourceType Type;
    public Sprite Icon;
    public string ResourceId => Type.ToString();
}

public class CatanHexTile : IHexTile
{
    public HexCoord Coord { get; }
    public CatanResourceType? ResourceType { get; }  // null = desert
    public int DiceNumber { get; }                    // 0 = desert
    public bool HasRobber { get; set; }

    // Called by CatanTurnManager when dice match this tile's number
    public void ProduceResources(Dictionary<HexVertex, Settlement> settlements);
}
```

Events from `CatanHexTile.ProduceResources`: `ResourceProducedEvent { CatanHexTile Tile; List<(IPlayer, ResourceBundle)> Productions }`

---

### Board

```csharp
public class CatanBoard
{
    public HexGrid<CatanHexTile> Grid { get; }
    public Dictionary<HexVertex, Settlement> Settlements { get; }
    public Dictionary<HexEdge, Road> Roads { get; }
    public HexCoord RobberPosition { get; private set; }
    public PortSystem Ports { get; }

    public void MoveRobber(HexCoord newPos);
    public List<CatanHexTile> GetTilesForNumber(int diceNumber);
    public List<IPlayer> GetPlayersOnTile(HexCoord coord);
}

public class PortSystem
{
    public List<Port> Ports { get; }
    public int GetTradeRatio(IPlayer player, CatanResourceType resource);
}

public class Port
{
    public CatanResourceType? SpecificResource;  // null = 3:1 generic
    public int TradeRatio;                        // 2 or 3
    public HexEdge Location;
    public HexVertex[] AccessVertices;            // 2 vertices player needs a settlement on
}

public class CatanBoardGenerator : BoardGenerator<CatanHexTile>
{
    public override HexGrid<CatanHexTile> Generate();  // randomises tiles + numbers + ports
}
```

---

### Buildable Pieces

```csharp
public class Settlement : IPlaceable, IBuildLocation
{
    public IPlayer Owner { get; }
    public HexVertex Location { get; }
    public bool IsCity { get; private set; }
    public int ProductionMultiplier => IsCity ? 2 : 1;

    // IBuildLocation — so Cities can "build on" settlements
    public bool IsOccupied => IsCity;
    public IPlaceable OccupiedBy => IsCity ? (IPlaceable)this : null;

    public void UpgradeToCity();

    public string PlaceableId => IsCity ? "city" : "settlement";
    public ResourceBundle BuildCost => IsCity
        ? new ResourceBundle().Add(Wheat, 2).Add(Ore, 3)
        : new ResourceBundle().Add(Wood, 1).Add(Brick, 1).Add(Sheep, 1).Add(Wheat, 1);
}

public class Road : IPlaceable
{
    public IPlayer Owner { get; }
    public HexEdge Location { get; }
    public string PlaceableId => "road";
    public ResourceBundle BuildCost => new ResourceBundle().Add(Wood, 1).Add(Brick, 1);
}
```

---

### Player

```csharp
public class CatanPlayer : IPlayer, ITurnActor
{
    // IPlayer
    public string Id { get; }
    public string DisplayName { get; }
    public Color Color { get; }

    // Resources — backed by ResourceInventory
    public ResourceInventory Resources { get; }

    // Cards
    public CardHand<DevelopmentCard> DevelopmentCards { get; }

    // Owned pieces
    public List<Settlement> Settlements { get; }
    public List<Road> Roads { get; }

    // Special achievements
    public int KnightsPlayed { get; internal set; }
    public bool HasLargestArmy { get; internal set; }
    public bool HasLongestRoad { get; internal set; }

    // Limits
    public const int MaxSettlements = 5;
    public const int MaxCities = 4;
    public const int MaxRoads = 15;

    public bool CanBuild(IPlaceable piece) =>
        Resources.CanAfford(piece.BuildCost) && HasPiecesRemaining(piece);
}
```

---

### Turn & Phases

```csharp
public enum CatanTurnPhase
{
    SetupPlacement,      // rounds 1 & 2, no dice
    RollDice,
    Robber,              // only when 7 rolled or knight played
    Trading,
    Building,
    EndTurn
}

public class CatanTurnManager : TurnManager
{
    public CatanTurnPhase CurrentCatanPhase { get; private set; }
    public DiceManager DiceManager { get; }
    public RobberSystem RobberSystem { get; }

    protected override void OnPhaseStart(CatanTurnPhase phase);
    public void HandleDiceRoll(int total);    // routes to resource production or robber
}

public class DiceManager
{
    public (int d1, int d2) LastRoll { get; private set; }
    public int Roll();      // returns d1 + d2, publishes DiceRolledEvent
}

public class RobberSystem
{
    public HexCoord CurrentPosition { get; private set; }
    public bool MustMove { get; private set; }

    public void Activate(IPlayer activePlayer);           // triggers discard check
    public void MoveRobber(HexCoord dest, IPlayer thief, IPlayer victim);
    public void ForceDiscard(IPlayer player, int count);  // > 7 cards rule
}
```

Events: `DiceRolledEvent { int D1, D2, Total }`, `SevenRolledEvent { IPlayer ActivePlayer }`, `RobberMovedEvent { HexCoord From, To; IPlayer Mover }`, `ResourceStolenEvent { IPlayer Thief, Victim; IResource Stolen }`, `DiscardRequiredEvent { IPlayer Player; int Count }`

---

### Rules

```csharp
public class CatanBuildRule : IBuildRule
{
    private CatanBoard _board;

    public bool CanPlace(IPlaceable piece, IBuildLocation location, IPlayer player)
    {
        return piece switch
        {
            Settlement s => ValidateSettlementPlacement(s, location, (CatanPlayer)player),
            Road r       => ValidateRoadPlacement(r, location, (CatanPlayer)player),
            _            => false
        };
    }

    // Settlement: unoccupied vertex + distance rule (no adjacent settlements) + road connection
    // City: must be own settlement (upgrade only)
    // Road: unoccupied edge + connected to own road or settlement
    private bool ValidateSettlementPlacement(...);
    private bool ValidateRoadPlacement(...);
}

public class CatanTradeRule : ITradeRule
{
    private CatanTurnManager _turn;
    private PortSystem _ports;

    public bool CanTrade(ITradeOffer offer, IPlayer responder)
    {
        if (_turn.CurrentCatanPhase != CatanTurnPhase.Trading) return false;
        if (offer.Target == null) return ValidateBankTrade(offer);   // bank/port trade
        return ValidatePlayerTrade(offer, responder);
    }

    // Bank trade: responder==null, ratio from PortSystem (2:1 / 3:1 / 4:1)
    private bool ValidateBankTrade(ITradeOffer offer);
    private bool ValidatePlayerTrade(ITradeOffer offer, IPlayer responder);
}
```

---

### Development Cards

```csharp
public abstract class DevelopmentCard : ICard
{
    public int TurnPurchased { get; set; }    // can't play same turn as bought
    public abstract bool IsPlayable(IGameContext context);
}

public class KnightCard : DevelopmentCard
{
    public override void OnPlay(IGameContext ctx)
    {
        ((CatanPlayer)ctx.ActivePlayer).KnightsPlayed++;
        ctx.RobberSystem.Activate(ctx.ActivePlayer);
        EventBus.Publish(new KnightPlayedEvent { Player = ctx.ActivePlayer });
    }
}

public class VictoryPointCard : DevelopmentCard
{
    public int Points => 1;
    public override bool IsPlayable(IGameContext ctx) => false;  // revealed at victory only
    public override void OnPlay(IGameContext ctx) { }
}

public class RoadBuildingCard : DevelopmentCard { ... }    // grants 2 free road placements
public class YearOfPlentyCard : DevelopmentCard { ... }    // grants 2 free resources
public class MonopolyCard : DevelopmentCard { ... }        // takes one resource type from all
```

---

### Score & Special Tokens

```csharp
public class CatanVictoryCondition : IVictoryCondition
{
    private CatanBoard _board;
    public int TargetPoints = 10;

    public int CalculatePoints(IPlayer p)
    {
        var cp = (CatanPlayer)p;
        return cp.Settlements.Count(s => !s.IsCity)  // 1 each
             + cp.Settlements.Count(s => s.IsCity) * 2
             + cp.DevelopmentCards.Cards.OfType<VictoryPointCard>().Sum(v => v.Points)
             + (cp.HasLargestArmy ? 2 : 0)
             + (cp.HasLongestRoad ? 2 : 0);
    }

    public bool IsWinCondition(IPlayer player) => CalculatePoints(player) >= TargetPoints;
}

public class LargestArmyTracker
{
    public IPlayer? CurrentHolder { get; private set; }
    public int CurrentCount { get; private set; }

    // Called on KnightPlayedEvent
    public void Update(IPlayer player, int knightsPlayed)
    {
        if (knightsPlayed >= 3 && knightsPlayed > CurrentCount)
        {
            // transfer token, publish LargestArmyChangedEvent
        }
    }
}

public class LongestRoadTracker
{
    public IPlayer? CurrentHolder { get; private set; }
    public int CurrentLength { get; private set; }

    // Called on BuildSucceededEvent (road) or RobberMovedEvent (breaking roads is not Catan,
    // but road removal after certain events needs recalculation)
    public void Recalculate(CatanBoard board)
    {
        // DFS/BFS through each player's road graph — longest contiguous path
    }
}
```

---

## Key Event Flows

### Dice Roll → Resource Production
```
Player clicks Roll
  → CatanTurnManager.RequestRoll()
  → DiceManager.Roll()
  → EventBus.Publish(DiceRolledEvent { 3, 4, 7 })
    → CatanTurnManager receives: 7 → transitions to Robber phase
                                 other → calls board.GetTilesForNumber(total)
      → each CatanHexTile.ProduceResources(settlements)
      → EventBus.Publish(ResourceProducedEvent { tile, [(player, bundle)] })
        → CatanPlayer.Resources.TryAdd(bundle)
        → EventBus.Publish(ResourceAddedEvent { player, bundle })
          → UI updates hand display
```

### Build Settlement
```
Player selects vertex
  → BuildManager.TryPlace(settlement, vertex, player)
  → CatanBuildRule.CanPlace(...)   [distance rule + road connection + resources]
  → if valid: player.Resources.TryRemove(settlement.BuildCost)
      → EventBus.Publish(ResourceRemovedEvent)
    → settlement placed on board
    → EventBus.Publish(BuildSucceededEvent { player, settlement, vertex })
      → LongestRoadTracker.Recalculate(board)
      → ScoreManager.RecalculateAll()
      → EventBus.Publish(ScoreChangedEvent)
        → ScoreManager.CheckVictory()
          → if 10 pts: EventBus.Publish(VictoryAchievedEvent { winner })
```

### Trade (Player ↔ Player)
```
Player A proposes trade
  → TradeManager.ProposeTradeToPlayer(A, B, offering, requesting)
  → EventBus.Publish(TradeProposedEvent { offer, target: B })
    → Player B's UI shows offer
Player B accepts
  → TradeManager.AcceptTrade(offer, B)
  → CatanTradeRule.CanTrade(offer, B)   [phase check, affordability]
  → A.Resources.TryRemove(offering) / B.Resources.TryAdd(offering)
  → B.Resources.TryRemove(requesting) / A.Resources.TryAdd(requesting)
  → EventBus.Publish(TradeCompletedEvent { offer })
    → UI clears trade panel, updates both hands
```

---

## Dependency Graph (no cycles)

```
com.gamecore.events          ← (no dependencies)
com.gamecore.statemachine    ← events
com.gamecore.player          ← events
com.gamecore.board           ← events
com.gamecore.turn            ← events, statemachine, player
com.gamecore.resources       ← events, player
com.gamecore.cards           ← events, player
com.gamecore.trade           ← events, player, resources
com.gamecore.build           ← events, player, resources, board
com.gamecore.score           ← events, player

Catan/                       ← all packages (extends interfaces, no circular deps)
```

---

## Extension Points

| Want to... | Where to hook in |
|---|---|
| Add multiplayer networking | Subscribe to EventBus, relay events over network; replay on remote clients |
| Add AI player | Implement `ITurnActor`, subscribe to phase events, call same public methods as human |
| Add Seafarers expansion | Extend `CatanBoard`, add new `TileType`, new `IVictoryCondition` |
| Port to a new board game | Reuse all packages, implement new `IHexTile`, `IPlayer`, `IBuildRule`, etc. |
| Add replay / undo | Log all `IGameEvent` instances; replay by re-publishing in order |
