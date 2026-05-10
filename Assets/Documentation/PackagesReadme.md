# Catan Unity — Package Overview

## Package Map

```
com.gamecore.events          ← no deps      foundation pub/sub
com.gamecore.statemachine    ← events        generic state machine
com.gamecore.player          ← events        player identity
com.gamecore.board           ← events        hex grid & topology
com.gamecore.turn            ← events, statemachine, player
com.gamecore.resources       ← events, player
com.gamecore.cards           ← events, player
com.gamecore.trade           ← events, player, resources
com.gamecore.build           ← events, player, resources, board
com.gamecore.score           ← events, player

Assets/Catan/                ← all packages  game implementation
```

No circular dependencies. The Catan layer is the only place that knows about all packages simultaneously.

---

## Package Responsibilities

| Package | One-line responsibility |
|---|---|
| `com.gamecore.events` | Static `EventBus` and ScriptableObject channels — the communication backbone |
| `com.gamecore.statemachine` | Generic `StateMachine<TContext>` used by turn flow, robber phase, and UI |
| `com.gamecore.player` | `IPlayer` identity contract and `PlayerManager` roster |
| `com.gamecore.board` | Axial hex math, `HexGrid<T>`, vertex/edge topology — no rendering |
| `com.gamecore.turn` | Whose turn it is, what phase is active, actor sequencing |
| `com.gamecore.resources` | Immutable `ResourceBundle` + mutable `ResourceInventory` with event publishing |
| `com.gamecore.cards` | Generic `CardDeck<T>` / `CardHand<T>`; cards implement their own effects |
| `com.gamecore.trade` | Offer lifecycle (propose → accept/reject → complete); rule injected externally |
| `com.gamecore.build` | Place/remove pieces on board locations; rule injected externally |
| `com.gamecore.score` | Sum pluggable `IVictoryCondition` contributions; detect winner |

---

## How the Packages Connect

### Communication backbone

Every package communicates through `EventBus.Publish<T>()`. No package holds a direct reference to another package's systems — only the Catan implementation layer (in `Assets/Catan/`) wires concrete types together via constructor injection and MonoBehaviour fields.

```
Game logic  →  EventBus.Publish<T>()  →  any number of subscribers
UI layer    →  subscribes to EventBus (or EventChannel in inspector)
```

### Data flow for a dice roll

```
CatanTurnManager.RequestRoll()
  → DiceManager.Roll()
  → EventBus.Publish(DiceRolledEvent)
    → CatanTurnManager: if 7 → transitions to Robber phase
                        else → CatanBoard.GetTilesForNumber(total)
                                 → CatanHexTile.ProduceResources(settlements)
                                 → ResourceInventory.TryAdd(bundle)
                                 → EventBus.Publish(ResourceAddedEvent)
                                   → UI updates hand display
```

### Data flow for building a settlement

```
Player clicks vertex
  → BuildManager.TryPlace(settlement, vertex, player)
  → CatanBuildRule.CanPlace(...)       validates distance rule + road connection
  → player.Resources.TryRemove(cost)  deducts wood/brick/sheep/wheat
  → EventBus.Publish(BuildSucceededEvent)
    → LongestRoadTracker.Recalculate(board)
    → ScoreManager.RecalculateAll()
    → EventBus.Publish(ScoreChangedEvent)
      → ScoreManager.CheckVictory()
        → if 10 pts: EventBus.Publish(VictoryAchievedEvent)
```

### Rule injection pattern

`BuildManager` and `TradeManager` own an `IBuildRule` / `ITradeRule` field. The Catan bootstrap code sets these to `CatanBuildRule` / `CatanTradeRule` at startup. This is the only point where Catan-specific logic enters the generic managers.

```csharp
// In GameManager.Start() or a bootstrap MonoBehaviour:
buildManager.Rule  = new CatanBuildRule(board);
tradeManager.Rule  = new CatanTradeRule(turnManager, board.Ports);
scoreManager.Conditions.Add(new CatanVictoryCondition(board));
```

---

## Suggested Unity Hierarchy

```
[Scene: CatanGame]

GameManager                        ← bootstrap; holds refs to all managers
  ├─ PlayerManager                 ← com.gamecore.player
  ├─ CatanTurnManager              ← drives DiceManager + RobberSystem internally
  ├─ BuildManager                  ← rule = CatanBuildRule
  ├─ TradeManager                  ← rule = CatanTradeRule
  └─ ScoreManager                  ← conditions = [CatanVictoryCondition, ...]

Board                              ← visual representation of CatanBoard data
  ├─ HexTile_(q,r)  ×19           ← one GameObject per tile; HexTileView component
  ├─ Vertices                      ← settlement/city click targets
  └─ Edges                         ← road click targets

UI (Canvas)
  ├─ HUD
  │   ├─ TurnIndicator             ← player name, color, phase label, turn counter
  │   └─ ScorePanel                ← per-player point totals
  ├─ PlayerHand                    ← resource counts + dev card list
  ├─ ActionPanel
  │   ├─ RollButton
  │   ├─ EndTurnButton
  │   ├─ BuildMenu                 ← settlement / city / road buttons + costs
  │   └─ DevCardButtons
  └─ TradePanel                    ← propose-to-player form + bank trade form

EventChannels (ScriptableObjects)  ← optional inspector-wired channels
  ├─ DiceRolledChannel
  ├─ ResourceProducedChannel
  └─ VictoryChannel
```

Each `HexTileView` subscribes to `TileSelectedEvent` for highlight. Each UI panel subscribes to the relevant `EventBus` events in `OnEnable` and unsubscribes in `OnDisable`. No UI component calls game logic directly — it dispatches input through the managers.
