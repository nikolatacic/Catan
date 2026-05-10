# Catan Unity — Project Specification & Status

**Branch**: `ai-gameplay-claude`  
**Unity version**: 6000.4.6f1  
**Target**: Local hotseat Catan for 2–4 players, playable in the Unity editor

---

## Linked Documents

| Document | Purpose |
|---|---|
| [ArchitecturePlan.md](ArchitecturePlan.md) | Full interface/class/event definitions for every package and the Catan layer |
| [ActionPlan.md](ActionPlan.md) | 8-phase implementation roadmap with acceptance criteria per phase |
| [PackagesReadme.md](PackagesReadme.md) | Package responsibilities, data flow diagrams, Unity hierarchy guide |

---

## Phase Status

| Phase | Title | Status | Notes |
|---|---|---|---|
| 1 | Scaffold all classes | **Done** | 98 files; all types exist as compilable stubs |
| 2 | Infrastructure (EventBus + StateMachine) | **Done** | Naming fixed, StateMachine publishes events, EditMode tests written |
| 3 | Core data (Board, Player, Resources) | **Done** | `HexGrid.BuildTopology` implemented; `PlayerManager` complete; tests written for ResourceBundle, ResourceInventory, PlayerManager, HexGrid |
| 4 | Catan board & pieces | **Done** | `CatanBoardGenerator` (19 tiles, 6/8 constraint, ports), `CatanHexTile.ProduceResources`, `CatanBoard`, `PortSystem.GetTradeRatio` all implemented; EditMode tests written |
| 5 | Turn system & dice | Not started | `CatanTurnManager`, `RobberSystem`, `DiceManager.Roll` all stubbed |
| 6 | Rules & game logic | Not started | `CatanBuildRule`, `CatanTradeRule`, dev cards, score trackers all stubbed |
| 7 | Unity scene & UI | Not started | No scene, no MonoBehaviour views yet |
| 8 | Polish & edge cases | Not started | |

---

## What Is Fully Implemented

| File | Status |
|---|---|
| `com.gamecore.events/Runtime/EventBus.cs` | Complete — subscribe, unsubscribe, publish with per-handler exception isolation |
| `com.gamecore.events/Runtime/GameEventChannel.cs` | Complete |
| `com.gamecore.statemachine/Runtime/StateMachine.cs` | Complete — Enter/Execute/Exit lifecycle, publishes StateEntered/StateExited/TransitionFired events |
| `com.gamecore.board/Runtime/HexCoord.cs` | Complete — axial math, neighbors, distance, world↔axial round-trip |
| `com.gamecore.resources/Runtime/ResourceBundle.cs` | Complete — immutable, `+` operator, `CanAfford` |
| `com.gamecore.resources/Runtime/ResourceInventory.cs` | Complete — TryAdd/TryRemove with event publishing |
| `com.gamecore.cards/Runtime/CardDeck.cs` | Complete — draw, shuffle, add-to-top/bottom |
| `com.gamecore.cards/Runtime/CardHand.cs` | Complete — add, remove, play with IsPlayable guard |
| `Assets/Catan/Score/CatanVictoryCondition.cs` | `CalculatePoints` complete; `IsWinCondition` complete |
| `Assets/Catan/Cards/VictoryPointCard.cs` | Complete |
| `Assets/Catan/Cards/KnightCard.cs` | `OnPlay` complete; `IsPlayable` stubbed |
| `Assets/Catan/Board/CatanResources.cs` | Complete — static registry, `Initialize()` + `Get(CatanResourceType)` |
| `Assets/Catan/Board/CatanHexTile.cs` | Complete — `ProduceResources` distributes to settlements/cities, publishes `ResourceProducedEvent` |
| `Assets/Catan/Board/CatanBoard.cs` | Complete — `MoveRobber`, `GetTilesForNumber`, `GetPlayersOnTile`, `ProduceResourcesForNumber` |
| `Assets/Catan/Board/CatanBoardGenerator.cs` | Complete — 19-tile layout, tile type shuffle, 6/8 adjacency constraint (200 retries), port system |
| `Assets/Catan/Board/PortSystem.cs` | Complete — `GetTradeRatio` returns best 2:1/3:1/4 ratio per player |
| `Assets/Catan/Build/Settlement.cs` | Complete — `BuildCost`, `ProductionMultiplier`, `UpgradeToCity` |
| `Assets/Catan/Build/Road.cs` | Complete — `BuildCost` |

---

## What Needs Implementation Next (Phase 2 → 3)

### Phase 2 — Tests

These classes work but have no tests yet:
- `EventBus`: subscribe → publish → called; unsubscribe → not called
- `StateMachine<T>`: transition calls Exit then Enter; Update calls Execute
- `GameEventChannel<T>`: Register/Raise/Unregister

### Phase 3 — HexGrid topology

`HexGrid.GetVertices(coord)` and `HexGrid.GetEdges(coord)` are stubbed. They need to return the shared `HexVertex` and `HexEdge` objects for a given tile. This requires building the vertex/edge index during board generation (in `CatanBoardGenerator.Generate()`), not on every query call.

---

## Key Architectural Decisions

| Decision | Rationale |
|---|---|
| EventBus is static, not a MonoBehaviour | Lets pure C# classes publish events without a Unity scene |
| ResourceBundle is immutable | Safe to pass as costs, offers, snapshots; no defensive copying needed |
| IBuildRule / ITradeRule are injected | Keeps generic managers reusable; Catan rules never leak into the packages |
| HexVertex / HexEdge are reference types | Settlements and roads hold references to the same object the grid holds — equality by reference |
| CatanTurnManager extends TurnManager | Catan-specific phase enum without duplicating turn sequencing logic |
| IGameContext passed into ICard.OnPlay | Cards are self-contained effects; no card holds a reference to the turn manager |

---

## File Layout Reference

```
Packages/
  com.gamecore.events/          README + EventBus, GameEventChannel, IGameEvent, IEventListener
  com.gamecore.statemachine/    README + StateMachine, IState, StateTransition, StateEvents
  com.gamecore.turn/            README + TurnManager, ITurnActor, TurnPhase, TurnEvents
  com.gamecore.board/           README + HexCoord, HexGrid, HexVertex, HexEdge, BoardGenerator, BoardEvents
  com.gamecore.player/          README + IPlayer, PlayerData, PlayerManager, PlayerEvents
  com.gamecore.resources/       README + IResource, ResourceBundle, ResourceInventory, ResourceEvents
  com.gamecore.cards/           README + ICard, IGameContext, CardDeck, CardHand, CardEvents
  com.gamecore.trade/           README + ITradeOffer, TradeOffer, ITradeRule, TradeManager, TradeEvents
  com.gamecore.build/           README + IPlaceable, IBuildLocation, IBuildRule, BuildManager, BuildEvents
  com.gamecore.score/           README + IVictoryCondition, ScoreManager, ScoreEvents

Assets/Catan/
  Board/         CatanResource, CatanResourceType, CatanHexTile, CatanBoard, CatanBoardGenerator, Port, PortSystem, BoardEvents
  Build/         Settlement, Road
  Cards/         DevelopmentCard, KnightCard, VictoryPointCard, RoadBuildingCard, YearOfPlentyCard, MonopolyCard
  Players/       CatanPlayer
  Rules/         CatanBuildRule, CatanTradeRule
  Score/         CatanVictoryCondition, LargestArmyTracker, LongestRoadTracker
  Turn/          CatanTurnPhase, CatanTurnManager, DiceManager, RobberSystem, TurnEvents
  ScriptableObjects/ CatanResourceChannel
  Catan.asmdef

Assets/Documentation/
  ArchitecturePlan.md
  ActionPlan.md
  PackagesReadme.md
  Specification.md   ← this file
```

---

## Important Constraints to Remember

- **Unsubscribe rule**: every `EventBus.Subscribe` must have a matching `Unsubscribe` in `OnDisable` or `OnDestroy`. Leaked handlers will fire after the subscriber is destroyed.
- **ResourceBundle immutability**: never mutate a bundle — always use `Add`/`Remove` which return new instances.
- **HexVertex identity**: the same `HexVertex` object is shared by 2–3 tiles. Once the topology is built during board generation, never create new vertex objects for the same intersection.
- **Turn phase gating**: `CatanTradeRule` and `CatanBuildRule` must check `CatanTurnManager.CurrentCatanPhase` before allowing any action. No action is valid outside its phase.
- **Dev card purchase restriction**: a dev card cannot be played the same turn it was purchased (`TurnPurchased == TurnNumber`).
- **Setup rounds**: during `SetupPlacement` phase, settlement placement does not require a road connection and does not cost resources. The 2nd settlement grants adjacent tile resources.
- **6/8 adjacency**: `CatanBoardGenerator` must ensure no two tiles with dice numbers 6 or 8 are adjacent — this is enforced during generation, not at play time.

---

## Session Log

| Date | Work done |
|---|---|
| 2026-05-10 | Created ArchitecturePlan.md (architecture reference document) |
| 2026-05-10 | Created ActionPlan.md (8-phase roadmap) |
| 2026-05-10 | Phase 1: scaffolded all 98 files — 10 packages + Assets/Catan/ layer + manifest.json |
| 2026-05-10 | Created per-package READMEs, PackagesReadme.md, Specification.md (this file) |
| 2026-05-10 | Phase 2: fixed naming violations (camelCase + full descriptive names), completed StateMachine event publishing, wrote EditMode tests for EventBus, GameEventChannel, StateMachine, HexCoord |
| 2026-05-10 | Phase 3: implemented HexGrid.BuildTopology (3-pass: create edges/vertices, wire adjacency, build per-tile lookup); completed PlayerManager; wrote EditMode tests for ResourceBundle, ResourceInventory, PlayerManager, HexGrid topology |
| 2026-05-10 | Phase 4: implemented CatanResources static registry, CatanHexTile.ProduceResources, CatanBoard (MoveRobber/GetTilesForNumber/GetPlayersOnTile/ProduceResourcesForNumber), CatanBoardGenerator (Fisher-Yates shuffle, 6/8 adjacency constraint, port system), PortSystem.GetTradeRatio, Settlement.BuildCost, Road.BuildCost; wrote EditMode tests for CatanBoardGenerator, CatanBoard, PortSystem |
