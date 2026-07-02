# Catan Unity — Documentation Index

Single entry point for all project documentation.  
**Active branches:** `new-ui-2-ui-toolkit` (UI Toolkit migration) · `multiplayertest` (multiplayer) · `main` (stable hotseat)  
**Engine:** Unity 6000.4.6f1

---

## Documents

| Document | What it covers |
|---|---|
| **[INDEX.md](INDEX.md)** | ← You are here |
| [PROGRESS.md](PROGRESS.md) | Phase completion status, repository layout, architecture rules, scene hierarchy, Inspector wiring, bug fix history |
| [ArchitecturePlan.md](ArchitecturePlan.md) | Full interface/class/event definitions for every package and the Catan layer; key event flows; dependency graph |
| [Specification.md](Specification.md) | Original project spec and per-file implementation status (hotseat game) |
| [ActionPlan.md](ActionPlan.md) | 8-phase implementation roadmap with acceptance criteria (hotseat game) |
| [PackagesReadme.md](PackagesReadme.md) | Package responsibilities, data flow diagrams, Unity hierarchy guide |
| [MultiplayerPlan.md](MultiplayerPlan.md) | All multiplayer phases (1–6), design decisions, NGO/Relay integration |
| [NetworkBoundaryAudit.md](NetworkBoundaryAudit.md) | Authority/Replica/Command classification of every system; events that leak hidden state |

---

## Quick reference — where to find X

| I want to know... | Go to |
|---|---|
| What packages exist and what each one does | [ArchitecturePlan.md §Package Structure](ArchitecturePlan.md) |
| How two systems communicate (events) | [ArchitecturePlan.md §Key Event Flows](ArchitecturePlan.md) |
| Which files to touch to add a new game action | [NetworkBoundaryAudit.md §Command surface](NetworkBoundaryAudit.md) |
| How to set up the Unity scene / Inspector wiring | [PROGRESS.md §Scene hierarchy](PROGRESS.md) |
| What bugs were fixed and why | [PROGRESS.md §Bug fix history](PROGRESS.md) |
| How multiplayer works end-to-end | [MultiplayerPlan.md](MultiplayerPlan.md) |
| Which systems run only on host vs every client | [NetworkBoundaryAudit.md §Game state](NetworkBoundaryAudit.md) |
| How to add a new build command in multiplayer | [NetworkBoundaryAudit.md §Command surface](NetworkBoundaryAudit.md) + [MultiplayerPlan.md §Phase 5b](MultiplayerPlan.md) |
| What's left to implement | [PROGRESS.md §Remaining work](PROGRESS.md) + [MultiplayerPlan.md §Phase 5e](MultiplayerPlan.md) |
| How phases of a turn sequence | [PROGRESS.md §Turn phase flow](PROGRESS.md) |

---

## Architecture in 60 seconds

```
Packages/com.gamecore.*/        Pure C# — no Unity, no Catan knowledge
  events                        EventBus (static pub/sub), foundation of everything
  board                         HexGrid, HexCoord, HexVertex, HexEdge
  turn                          TurnManager, ITurnActor
  resources                     ResourceBundle (immutable), ResourceInventory
  build                         BuildManager, IBuildRule, IPlaceable
  trade                         TradeManager, ITradeRule
  score                         ScoreManager, IVictoryCondition
  cards                         CardDeck<T>, CardHand<T>
  player                        IPlayer interface
  statemachine                  StateMachine<T>

Assets/Catan/                   Catan-specific layer (implements gamecore interfaces)
  Board/                        CatanBoard, CatanHexTile, CatanBoardGenerator, PortSystem
  Build/                        Settlement, Road, CityUpgrade
  Players/                      CatanPlayer, CatanResources
  Turn/                         CatanTurnManager, DiceManager, RobberSystem
  Rules/                        CatanBuildRule, CatanTradeRule, CatanVictoryCondition
  Cards/                        KnightCard, VictoryPointCard, RoadBuildingCard, YearOfPlentyCard, MonopolyCard
  Score/                        LargestArmyTracker, LongestRoadTracker
  Commands/                     IGameCommand, CommandDispatcher, 10 concrete commands
  Session/                      GameSession, NetworkSession (static state carriers)
  Network/                      NetworkCommandBridge, NetworkEventBridge, BoardKeys
  UI/                           All MonoBehaviour views — read state, render, dispatch commands
  Editor/                       Editor scripts that generate prefabs and scene objects
```

**The three rules everything follows:**
1. **Logic never touches Unity.** The 10 `com.gamecore.*` packages and most of `Assets/Catan/` are plain C#. MonoBehaviours live only in `Assets/Catan/UI/`.
2. **Systems talk through EventBus, not direct references.** `EventBus.Publish<T>()` is synchronous — state must be set *before* publishing.
3. **In multiplayer, only the host mutates state.** Clients send `IGameCommand` via `CommandDispatcher` → ServerRpc → host executes → `NetworkEventBridge` fans out events to all clients.

---

## Multiplayer in 60 seconds

```
MainMenu scene                  NetworkManager lives here (DontDestroyOnLoad)
  LobbyView                     Host: allocate Relay → StartHost → show code → LoadScene
                                Client: enter code → JoinAllocation → StartClient → follow host

GameHotseat scene               Loaded by host via NGO SceneManager (both peers land here)
  NetworkBridges                Single GameObject with NetworkObject + two components:
    NetworkCommandBridge        10 [ServerRpc] methods — client intents arrive here, host executes
    NetworkEventBridge          Subscribes to host EventBus → fans out via [ClientRpc] → clients re-publish locally

Static state (survives scene loads)
  NetworkSession                Mode (Hotseat/Host/Client), LocalPlayerIndex, JoinCode
  GameSession                   PlayerConfig list for CreatePlayers()

Client init sequence
  1. Scene loads → GameManager.Start() → IsClient=true → defers init
  2. NetworkEventBridge.OnNetworkSpawn → RequestInitialStateServerRpc()
  3. Host responds: SendInitialStateClientRpc(seed, playerIndex, actorIndex, turnNumber, phase)
  4. Client: CompleteInitialization(seed) → board renders
  5. Client: LocalPlayerIndex set → LocalPlayerAssignedEvent → UI gates activated
```

---

## Dependency graph (no cycles allowed)

```
events  ←  statemachine
events  ←  player
events  ←  board
events  ←  turn        ←  player
events  ←  resources   ←  player
events  ←  cards       ←  player
events  ←  trade       ←  player, resources
events  ←  build       ←  player, resources, board
events  ←  score       ←  player

Catan/  ←  all packages  (implements interfaces, no circular deps)
```

---

## Adding a new feature — checklist

**New game action (e.g. player-to-player trade):**
1. Add `Try*(...)` method to `GameManager`
2. Create `XxxCommand : IGameCommand` in `Assets/Catan/Commands/`
3. Add matching `[ServerRpc]` to `NetworkCommandBridge`; implement `SendOverNetwork`
4. If it produces an event: add corresponding `[ClientRpc]` in `NetworkEventBridge`
5. Wire the UI caller to `CommandDispatcher.Send(new XxxCommand(...))`

**New event:**
1. Define `public class XxxEvent : IGameEvent { ... }` (anywhere — events are simple structs/classes)
2. Publish: `EventBus.Publish(new XxxEvent { ... })`
3. Subscribe in the interested component: `EventBus.Subscribe<XxxEvent>(OnXxx)` + matching `Unsubscribe`
4. In multiplayer: add a host handler + ClientRpc in `NetworkEventBridge`
