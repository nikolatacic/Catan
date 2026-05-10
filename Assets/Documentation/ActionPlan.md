# Catan Unity — Action Plan

## Overview

This document translates the Architecture Plan into a concrete, ordered implementation plan.
Each phase builds on the previous; no phase should begin until the prior one compiles cleanly.

**Target**: Fully playable local-multiplayer Catan in Unity 6 with the event-driven, package-based architecture defined in `ArchitecturePlan.md`.

**Branch**: `ai-gameplay-claude`

---

## Phase 1 — Scaffold All Classes (Skeleton Code)

**Goal**: Every class, interface, enum, and event type in the Architecture Plan exists as a compilable file with the correct signature. No logic yet — just stubs that compile. This phase establishes the file/folder layout and dependency graph.

### 1.1 Create Package Directories

Create each local Unity package with a `package.json` and `Runtime/` folder:

```
Packages/
  com.gamecore.events/
    package.json
    Runtime/
      EventBus.cs
      GameEventChannel.cs
      IGameEvent.cs
      IEventListener.cs

  com.gamecore.statemachine/
    package.json
    Runtime/
      IState.cs
      StateMachine.cs
      StateTransition.cs
      StateEvents.cs          # StateEnteredEvent<T>, StateExitedEvent<T>, TransitionFiredEvent<T>

  com.gamecore.turn/
    package.json
    Runtime/
      ITurnActor.cs
      TurnManager.cs
      TurnPhase.cs
      TurnEvents.cs           # TurnStartedEvent, TurnEndedEvent, PhaseChangedEvent, ActorSkippedEvent

  com.gamecore.board/
    package.json
    Runtime/
      HexCoord.cs
      IHexTile.cs
      HexVertex.cs
      HexEdge.cs
      HexGrid.cs
      BoardGenerator.cs
      BoardEvents.cs          # TileSelectedEvent, VertexSelectedEvent, EdgeSelectedEvent

  com.gamecore.player/
    package.json
    Runtime/
      IPlayer.cs
      PlayerData.cs
      PlayerManager.cs
      PlayerEvents.cs         # PlayerJoinedEvent, PlayerLeftEvent, ActivePlayerChangedEvent

  com.gamecore.resources/
    package.json
    Runtime/
      IResource.cs
      ResourceBundle.cs
      IResourceInventory.cs
      ResourceInventory.cs
      ResourceEvents.cs       # ResourceAddedEvent, ResourceRemovedEvent, ResourceInsufficientEvent, ResourceTransferredEvent

  com.gamecore.cards/
    package.json
    Runtime/
      ICard.cs
      IGameContext.cs
      CardDeck.cs
      CardHand.cs
      CardEvents.cs           # CardDrawnEvent<T>, CardPlayedEvent<T>, CardDiscardedEvent<T>, DeckEmptyEvent

  com.gamecore.trade/
    package.json
    Runtime/
      ITradeOffer.cs
      ITradeRule.cs
      TradeStatus.cs
      TradeOffer.cs
      TradeManager.cs
      TradeEvents.cs          # TradeProposedEvent, TradeAcceptedEvent, TradeRejectedEvent, TradeCompletedEvent, TradeCancelledEvent

  com.gamecore.build/
    package.json
    Runtime/
      IPlaceable.cs
      IBuildLocation.cs
      IBuildRule.cs
      BuildManager.cs
      BuildEvents.cs          # BuildAttemptedEvent, BuildSucceededEvent, BuildFailedEvent, PieceRemovedEvent

  com.gamecore.score/
    package.json
    Runtime/
      IVictoryCondition.cs
      ScoreManager.cs
      ScoreEvents.cs          # ScoreChangedEvent, LeaderChangedEvent, VictoryAchievedEvent
```

### 1.2 Create Catan Implementation Directories

```
Assets/Catan/
  Board/
    CatanResource.cs
    CatanResourceType.cs
    CatanHexTile.cs
    CatanBoard.cs
    CatanBoardGenerator.cs
    PortSystem.cs
    Port.cs
    BoardEvents.cs            # ResourceProducedEvent

  Players/
    CatanPlayer.cs

  Build/
    Settlement.cs
    Road.cs

  Turn/
    CatanTurnPhase.cs
    CatanTurnManager.cs
    DiceManager.cs
    RobberSystem.cs
    TurnEvents.cs             # DiceRolledEvent, SevenRolledEvent, RobberMovedEvent, ResourceStolenEvent, DiscardRequiredEvent

  Cards/
    DevelopmentCard.cs
    KnightCard.cs
    VictoryPointCard.cs
    RoadBuildingCard.cs
    YearOfPlentyCard.cs
    MonopolyCard.cs

  Rules/
    CatanBuildRule.cs
    CatanTradeRule.cs

  Score/
    CatanVictoryCondition.cs
    LargestArmyTracker.cs
    LongestRoadTracker.cs

  UI/                         # empty — populated in Phase 6

  ScriptableObjects/
    CatanResourceChannel.cs   # typed EventChannel for inspector wiring
```

### 1.3 Register Packages in manifest.json

Add all `com.gamecore.*` packages as local file references:

```json
"com.gamecore.events": "file:../Packages/com.gamecore.events",
"com.gamecore.statemachine": "file:../Packages/com.gamecore.statemachine",
...
```

### Acceptance Criteria — Phase 1

- [ ] Unity compiles with zero errors
- [ ] All interfaces, classes, enums, and event types exist as files
- [ ] No logic required — `throw new NotImplementedException()` is acceptable in method bodies
- [ ] Package dependency graph matches `ArchitecturePlan.md` (no circular deps)

---

## Phase 2 — Infrastructure Packages (Events + State Machine)

**Goal**: `com.gamecore.events` and `com.gamecore.statemachine` are fully working with tests.

### 2.1 EventBus

Implement `EventBus` with thread-safe handler registration and publish/unsubscribe.

```csharp
// Key behaviours to implement:
EventBus.Subscribe<T>(handler)   // registers handler for event type T
EventBus.Unsubscribe<T>(handler) // deregisters
EventBus.Publish<T>(evt)         // calls all registered handlers for T
```

- Use `Dictionary<Type, Delegate>` internally
- Guard against handlers modifying the collection during iteration (snapshot before iterate)
- Guard against exceptions in one handler silencing others (try/catch per handler, log errors)

### 2.2 ScriptableObject EventChannel

Implement `GameEventChannel<T>` as `ScriptableObject`:

- `Raise(T value)` — calls all registered `UnityAction<T>` listeners
- `Register` / `Unregister` — safe to call in `OnEnable` / `OnDisable`
- Create concrete subclass `[CreateAssetMenu]` for each event type used in inspector wiring

### 2.3 StateMachine

Implement `StateMachine<TContext>`:

- `Transition(newState)` — calls `CurrentState.Exit(ctx)`, sets new state, calls `Enter(ctx)`
- `Update()` — calls `CurrentState.Execute(ctx)`
- Publish `StateEnteredEvent<T>`, `StateExitedEvent<T>` on transitions
- Guard against null states and re-entrant transitions

### 2.4 Tests

Create `EditMode` test assembly for each package:

- `EventBus`: subscribe → publish → handler called; unsubscribe → publish → handler NOT called
- `StateMachine`: transition calls Exit/Enter in correct order; Update calls Execute

### Acceptance Criteria — Phase 2

- [ ] `EventBus` passes all unit tests
- [ ] `StateMachine` passes all unit tests
- [ ] `GameEventChannel<T>` can be created as a ScriptableObject in the editor

---

## Phase 3 — Core Data Packages (Board, Player, Resources)

**Goal**: The pure-data packages are fully implemented. No MonoBehaviour managers yet.

### 3.1 HexCoord & HexGrid

- Implement axial coordinate arithmetic: `Neighbors()`, `Distance()`, `ToWorldPosition()`, `FromWorldPosition()`
- `HexGrid<T>`: tile storage, neighbour lookup, vertex and edge enumeration
- `HexVertex` and `HexEdge`: adjacency tables built when grid is populated

### 3.2 ResourceBundle

- Immutable value object backed by `IReadOnlyDictionary<IResource, int>`
- `Add`, `Remove` return new instances (no mutation)
- `CanAfford` checks all resource amounts ≥ requested
- `operator +` merges two bundles

### 3.3 ResourceInventory

- Mutable, wraps a `ResourceBundle`
- `TryAdd` / `TryRemove` publish `ResourceAddedEvent` / `ResourceRemovedEvent` / `ResourceInsufficientEvent`

### 3.4 PlayerData ScriptableObject

- Implements `IPlayer` with serialized `Id`, `DisplayName`, `Color`

### 3.5 Tests

- HexCoord: neighbor count, distance, axial↔world round-trip
- ResourceBundle: add/remove immutability, CanAfford edge cases (zero, exact, over)
- ResourceInventory: events fired on add/remove

### Acceptance Criteria — Phase 3

- [ ] Hex math unit tests pass
- [ ] ResourceBundle is immutable (verified by test)
- [ ] ResourceInventory publishes correct events

---

## Phase 4 — Catan Board & Pieces

**Goal**: A standard Catan board can be generated with tiles, ports, vertices, and edges correctly wired.

### 4.1 CatanHexTile

- Stores `CatanResourceType?`, `DiceNumber`, `HasRobber`
- `ProduceResources(settlements)`: iterates settlements on adjacent vertices, adds resources proportional to `ProductionMultiplier`, publishes `ResourceProducedEvent`

### 4.2 CatanBoardGenerator

Standard 19-tile layout:

- Fixed ring counts: 1 center + 6 ring-1 + 12 ring-2
- Shuffle tile types according to Catan distribution: 4 wood, 4 sheep, 4 wheat, 3 ore, 3 brick, 1 desert
- Assign numbers (2–12, no 7) using official Catan letter system (A–R spiral), ensure 6 and 8 are not adjacent
- Assign 9 ports: 4 generic (3:1), 5 specific (2:1 for each resource), at border edges
- Robber starts on desert

### 4.3 Settlement & Road

- `Settlement`: implements both `IPlaceable` and `IBuildLocation`; `UpgradeToCity()` sets `IsCity = true`, recalculates `BuildCost`
- `Road`: implements `IPlaceable`

### 4.4 PortSystem

- `GetTradeRatio(player, resource)`: checks player's settlements against port access vertices, returns best ratio (2, 3, or 4)

### 4.5 CatanBoard

- Owns the `HexGrid<CatanHexTile>`, `Settlements` dict, `Roads` dict, robber position, `PortSystem`
- `GetTilesForNumber(n)`: returns all tiles where `DiceNumber == n` and `!HasRobber`
- `GetPlayersOnTile(coord)`: returns distinct owners of settlements on tile's vertices
- `MoveRobber(coord)`: sets `HasRobber` on old/new tiles

### 4.6 Tests

- Generator: 19 tiles, correct type distribution, no adjacent 6/8, 9 ports
- PortSystem: correct ratio returned for each settlement position
- ProduceResources: correct amounts per settlement/city

### Acceptance Criteria — Phase 4

- [ ] Board generates without assertion errors
- [ ] Distribution and adjacency tests pass
- [ ] `ProduceResources` correctly scales for cities

---

## Phase 5 — Turn System & Dice

**Goal**: Turn flow works end-to-end in pure C# (no scene needed).

### 5.1 CatanPlayer

- Fully implement `CatanPlayer`: `ResourceInventory`, `CardHand<DevelopmentCard>`, piece lists, `CanBuild()`, piece limits
- Implements `ITurnActor`: `CanAct` returns true when it is this player's turn

### 5.2 CatanTurnManager

- Inherits from `TurnManager`; manages `CatanTurnPhase` state machine
- Setup rounds: players place in forward order, then reverse order (snake draft)
- `HandleDiceRoll(total)`:
  - 7 → transition to `Robber` phase, activate `RobberSystem`
  - other → call `board.GetTilesForNumber(total)`, call `ProduceResources` on each
- Phase transitions: `RollDice → Trading → Building → EndTurn → (next player) RollDice`

### 5.3 DiceManager

- `Roll()`: generate two `Random.Range(1,7)`, store, publish `DiceRolledEvent`, return sum

### 5.4 RobberSystem

- `Activate(activePlayer)`: check all players with >7 cards, publish `DiscardRequiredEvent` for each
- `MoveRobber(dest, thief, victim)`: update board, steal one random resource from victim, publish `RobberMovedEvent` + `ResourceStolenEvent`

### 5.5 PlayerManager & TurnManager MonoBehaviour Runners

- `PlayerManager` MonoBehaviour: holds `List<CatanPlayer>`, wires them into `CatanTurnManager`
- `TurnManager` tick: call `Update()` on state machine each frame (or via button in Phase 6)

### 5.6 Tests

- Turn order: N players → correct sequence, snake draft order
- Dice: 1000 rolls stay in [2,12]; `DiceRolledEvent` always fired
- Robber: discard threshold, resource steal removes from victim and adds to thief

### Acceptance Criteria — Phase 5

- [ ] Full turn cycle runs in an EditMode or PlayMode test
- [ ] `DiscardRequiredEvent` fires exactly for players with >7 cards
- [ ] `ResourceStolenEvent` carries correct resources

---

## Phase 6 — Rules & Game Logic

**Goal**: All Catan rules are enforced. `BuildManager` and `TradeManager` are operational.

### 6.1 CatanBuildRule

- Settlement placement: vertex unoccupied + distance rule (no adjacent settlement) + must connect to own road (except setup rounds)
- City placement: vertex occupied by own settlement
- Road placement: edge unoccupied + connected to own road or settlement + not blocked by opponent settlement at junction

### 6.2 CatanTradeRule

- Only valid during `Trading` phase
- Player trade: both players can afford the exchange; proposer has resources offered
- Bank trade: ratio from `PortSystem`; bank has infinite resources

### 6.3 BuildManager

- `TryPlace`: run `IBuildRule.CanPlace` → deduct resources → place piece → publish `BuildSucceededEvent` or `BuildFailedEvent`
- After road placed: trigger `LongestRoadTracker.Recalculate`
- After settlement/city placed: trigger `ScoreManager.RecalculateAll`

### 6.4 TradeManager

- Full negotiation loop: propose → accept/reject/counter → complete/cancel
- `ProposeTradeToBank`: apply port ratio, execute immediately on acceptance

### 6.5 Development Cards

- `CardDeck<DevelopmentCard>`: shuffle on game start (14 knight, 5 VP, 2 road building, 2 year of plenty, 2 monopoly)
- Purchase: player pays ore+wheat+sheep, draws top card, sets `TurnPurchased`
- `IsPlayable`: card is not played same turn as purchased; player hasn't played a dev card this turn (except VP)
- Implement each card's `OnPlay`

### 6.6 Score & Special Tokens

- `LargestArmyTracker`: on `KnightPlayedEvent`, recalculate; transfer token if threshold met (≥3 knights, more than current holder)
- `LongestRoadTracker`: DFS from each road segment, find longest contiguous path per player; transfer token at ≥5 roads
- `ScoreManager.RecalculateAll`: sum all `IVictoryCondition.CalculatePoints` for each player, publish `ScoreChangedEvent` if changed, call `CheckVictory()`

### 6.7 Tests

- Build rule: distance rule blocks adjacent settlements; road must connect
- Trade rule: bank ratio, phase gate
- Card play: same-turn block, knight activates robber
- Longest road: branching paths, interrupted by opponent settlement

### Acceptance Criteria — Phase 6

- [ ] All build rule tests pass
- [ ] Trade completes and transfers resources correctly
- [ ] Knight card triggers robber flow
- [ ] Largest army and longest road transfer at correct thresholds
- [ ] Victory fires at exactly 10 points

---

## Phase 7 — Unity Scene & Basic UI

**Goal**: The game can be played in the Unity editor with a minimal but functional UI.

### 7.1 Scene Setup

- Main scene: `GameManager` GameObject wiring all MonoBehaviour managers together
- Camera: orthographic, centered on board
- Board renderer: one `GameObject` per hex tile, positioned via `HexCoord.ToWorldPosition()`

### 7.2 Board Visuals

- `HexTileView` MonoBehaviour: subscribes to `TileSelectedEvent`, highlights tile
- Tile sprites/materials per `CatanResourceType` (use existing texture assets)
- Number tokens rendered with TextMesh Pro
- Robber visual: icon moves on `RobberMovedEvent`

### 7.3 Player Hand UI

- `PlayerHandView`: subscribes to `ResourceAddedEvent` / `ResourceRemovedEvent`, updates resource counts per player
- Dev card list: updates on `CardDrawnEvent` / `CardPlayedEvent`
- Score panel: updates on `ScoreChangedEvent`

### 7.4 Action Buttons

- Roll Dice button (active only in `RollDice` phase)
- End Turn button (active only in `Building` phase)
- Build menu: settlement / city / road buttons with cost display; disabled when player can't afford or no valid locations
- Trade panel: propose trade form for player and bank trades
- Dev card play buttons

### 7.5 Turn Indicator

- Active player name and color shown
- Phase label (Rolling / Trading / Building)
- Turn number counter

### 7.6 Interaction

- Vertex click → attempt settlement placement if in Building phase
- Edge click → attempt road placement
- Tile click during Robber phase → move robber (prompt to steal from valid players)

### Acceptance Criteria — Phase 7

- [ ] Full game playable from start to victory in editor (2–4 players, hotseat)
- [ ] No null reference exceptions during normal play
- [ ] Visual state matches game state (hand counts, scores, piece positions)
- [ ] Victory screen shown on `VictoryAchievedEvent`

---

## Phase 8 — Polish & Edge Cases

**Goal**: All known edge cases handled; game is stable for a full session.

### 8.1 Edge Cases to Handle

| Scenario | Expected Behavior |
|---|---|
| Player can't afford to discard (has exactly 7 cards) | No discard required |
| All dev cards bought | Purchase disabled, no draw |
| Player has 0 settlements remaining | Placement disabled |
| Tie for longest road / largest army | No token transfer |
| Road building card with 0 valid road spots | Card playable, 0 roads placed |
| Bank runs out of a resource type | Partial production (only give what bank has) |
| Monopoly on resource nobody has | No effect, no error |
| Setup: 2nd settlement resource grant | Each player gets resources from 2nd settlement's adjacent tiles |

### 8.2 Input Validation

- Block all actions out of phase
- Graceful error messages (not exceptions) when invalid action attempted

### 8.3 Stability

- No `Update()` loop logic that could diverge (only event-driven state changes)
- Memory leak check: all `EventBus.Unsubscribe` calls in `OnDisable` / `OnDestroy`

### Acceptance Criteria — Phase 8

- [ ] All edge cases in the table above behave correctly
- [ ] 3 complete games played without error or freeze
- [ ] No leaked event subscriptions (verified by teardown test)

---

## Dependency Graph (Implementation Order)

```
Phase 1  ──── Scaffold (all files, no logic)
Phase 2  ──── Events + StateMachine          [no game deps]
Phase 3  ──── Board data + Player + Resources [uses Phase 2]
Phase 4  ──── CatanBoard + Pieces             [uses Phase 3]
Phase 5  ──── Turn system + Dice + Robber     [uses Phases 3–4]
Phase 6  ──── Rules + Cards + Score           [uses Phases 3–5]
Phase 7  ──── Unity scene + UI                [uses Phases 3–6]
Phase 8  ──── Edge cases + polish             [uses Phases 3–7]
```

---

## File Count Estimate

| Location | Files |
|---|---|
| `com.gamecore.*` packages (10 packages × ~5 files) | ~50 |
| `Assets/Catan/` implementation layer | ~35 |
| Test assemblies | ~20 |
| **Total** | **~105** |

---

## Definition of Done

The implementation is complete when:

1. A 2–4 player hotseat game of Catan can be played to completion in the Unity editor
2. All phases 1–8 acceptance criteria are met
3. Zero compiler warnings in Release configuration
4. `EventBus.Unsubscribe` called for every `Subscribe` (no leaked handlers)
5. Architecture matches the dependency graph in `ArchitecturePlan.md` (verified by package `asmdef` references)
