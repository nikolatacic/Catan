# Catan Unity — Project Progress

**Active branch**: `multiplayertest` (all current work here)  
**Unity**: 6000.4.6f1 — hotseat + Unity Relay networked multiplayer  
**Last updated**: 2026-06-26

---

## What this is

Local hotseat Catan implementation built from scratch in Unity 6 using an event-driven package architecture. All game logic lives in pure C# packages under `Packages/com.gamecore.*/`; MonoBehaviours are UI-only under `Assets/Catan/UI/`.

---

## Phase completion

### Hotseat game (original)
| Phase | Status | Description |
|---|---|---|
| 1 | Done | 98 files scaffolded across 10 packages |
| 2 | Done | EventBus, StateMachine, EditMode tests |
| 3 | Done | HexGrid topology, PlayerManager, core data tests |
| 4 | Done | CatanBoardGenerator, CatanHexTile, PortSystem, tests |
| 5 | Done | TurnManager, CatanTurnManager, DiceManager, RobberSystem, tests |
| 6 | Done | CatanBuildRule, CatanTradeRule, BuildManager, TradeManager, ScoreManager, LargestArmyTracker, LongestRoadTracker, all dev cards, CatanGameContext, tests |
| 7 | Done | All UI MonoBehaviours written and gameplay working |
| 8 | In progress | Polish and edge cases — several items done, see below |

### Multiplayer (Unity Relay + NGO, branch: `multiplayertest`)
| Phase | Status | Description |
|---|---|---|
| MP-1 | Done | Scene separation: MainMenu + GameHotseat |
| MP-2 | Done | GameSession static carrier for player configs across scenes |
| MP-3 | Done | Network boundary audit (see NetworkBoundaryAudit.md) |
| MP-4 | Done | Command pattern: IGameCommand + CommandDispatcher + 10 commands |
| MP-5a | Done | Lobby scaffolding: LobbyView, NetworkSession, Relay allocation |
| MP-5b | Done | NetworkCommandBridge: 10 ServerRpc methods, vertex/edge encoding |
| MP-5c | Done | NetworkEventBridge: seed sync + EventBus fan-out to all clients |
| MP-5d | Done | Per-device UI: index assignment, LocalPlayerAssignedEvent, MirrorActor/MirrorPhase |
| MP-5e | Not started | Hidden state filtering, real lobby player picker, StealTargetPanel for clients |
| MP-6 | Not started | Final polish, edge cases |

---

## Repository layout

```
Packages/com.gamecore.*/          Pure C# packages (no Unity dependencies)
  com.gamecore.board/             HexGrid, HexVertex, HexEdge, HexCoord
  com.gamecore.events/            EventBus (static, synchronous pub/sub)
  com.gamecore.resources/         ResourceBundle (immutable), ResourceInventory
  com.gamecore.build/             BuildManager, IBuildLocation, IPlaceable
  com.gamecore.trade/             TradeManager, TradeOffer, ITradeRule
  com.gamecore.score/             ScoreManager, IVictoryCondition
  com.gamecore.turn/              TurnManager, ITurnActor, DiceManager
  com.gamecore.cards/             CardHand, CardDeck, DevelopmentCard
  com.gamecore.player/            IPlayer
  com.gamecore.statemachine/      StateMachine

Assets/Catan/                     Catan-specific game logic
  Board/                          CatanBoard, CatanHexTile, CatanBoardGenerator, PortSystem
  Build/                          Settlement, Road, CatanBuildRule
  Cards/                          KnightCard, VictoryPointCard, RoadBuildingCard, YearOfPlentyCard, MonopolyCard
  Players/                        CatanPlayer, CatanResources (ScriptableObject)
  Rules/                          CatanTradeRule, CatanVictoryCondition
  Score/                          LargestArmyTracker, LongestRoadTracker
  Turn/                           CatanTurnManager, RobberSystem, DiceManager
  UI/                             All MonoBehaviour view components
  Editor/                         Editor scripts (prefab generators)
  Tests/EditMode/                 NUnit tests
```

---

## Architecture rules

- **EventBus** is static and synchronous — handlers fire inside the `Publish` call. State must be set BEFORE calling anything that publishes.
- **GameManager** (singleton MonoBehaviour) is the only orchestrator. View components call `GameManager.Instance.DoX()` — they never touch systems directly.
- **ResourceBundle** is immutable — `Add(resource, amount)` and `Remove(resource, amount)` return new instances.
- **IPlayer** has no `Resources` property — cast to `CatanPlayer` to access resources.
- **CardHand\<T\>** exposes `.Cards` (IReadOnlyList), not `.Count`.
- **HexVertex.AdjacentTiles** is `HexCoord[]` (not tile objects) — use `Board.Grid.Tiles.TryGetValue(coord, out var tile)`.
- **static readonly IResource[] fields** must not capture `CatanResources.Wood` etc. at class load time (they are null until `CatanResources.Initialize()` runs in `GameManager.Start()`). Use a property (`=>`) instead.
- **Panels that start inactive**: subscribe in `Awake` + unsubscribe in `OnDestroy` — NOT `OnEnable`/`OnDisable`. `OnEnable` never fires if `Awake` calls `SetActive(false)`.
- **Assembly**: all Catan logic is in `Catan` asmdef. Editor scripts in `Catan.Editor` asmdef (Editor-only, references `Catan` + `Unity.TextMeshPro`).

---

## Key files

### UI layer — `Assets/Catan/UI/`

| File | Role |
|---|---|
| `GameManager.cs` | Singleton root; wires all systems in `Start()`; exposes `TryPlaceSettlement`, `TryPlaceRoad`, `TryUpgradeCity`, `TryMoveRobber`, `TryBankTrade`, `TryPurchaseDevCard`, `TryPlayDevCard`, `EndTurn`, `RequestRoll`, placement mode setters |
| `BoardRenderer.cs` | Spawns tile/vertex/edge GameObjects; vertex world pos = `center + (cos(-60i°), sin(-60i°)) * HexSize`; edge i rotated `−60*(i+1)°`; driven by `GameManager.Start()`, no `Start()` of its own |
| `HexTileView.cs` | `OnMouseDown` → `TryMoveRobber` during Robber phase |
| `VertexView.cs` | `OnMouseDown` → `TryPlaceSettlement` or `TryUpgradeCity` |
| `EdgeView.cs` | `OnMouseDown` → `TryPlaceRoad` |
| `PlayerHandView.cs` | Resource counts; subscribes to `ResourceAddedEvent`/`ResourceRemovedEvent`; field is `Player`, not `Owner` |
| `TurnIndicatorView.cs` | Active player, phase, turn number, dice result (`DiceResultLabel` TMP field — wire in Inspector) |
| `ActionButtonsView.cs` | Phase-gated buttons; `BankTradeButton` enabled during Trading/Building; `BankTradePanel` ref opens `BankTradePanelView` |
| `RobberView.cs` | Single world-space robber icon; `SnapToCurrentPosition()` called by `GameManager.Start()` |
| `VictoryScreenView.cs` | Activates on `VictoryAchievedEvent` |
| `DiscardPanelView.cs` | Activates on `DiscardRequiredEvent`; queues multiple players; `OnConfirmDiscard()` calls `player.Resources.TryRemove(bundle)` |
| `DiscardCardView.cs` | Clickable card widget; colour-coded by `CatanResourceType` |
| `BankTradePanelView.cs` | 5 give + 5 receive buttons; shows count + ratio; calls `GameManager.TryBankTrade(give, receive)` |

### Logic layer — `Assets/Catan/`

| File | Key details |
|---|---|
| `Turn/CatanTurnManager.cs` | Snake-draft setup queue; `HandleDiceRoll` routes 7→Robber or distribute; `IsSecondSetupRound` (true after first N setup turns); `BeginKnightRobberPhase()` captures `_phaseAfterRobber` then switches to Robber; `AdvancePhase` returns to `_phaseAfterRobber` after Robber (dice-7 sets it to Building, Knight sets it to wherever the player was) |
| `Turn/RobberSystem.cs` | `Activate` publishes `SevenRolledEvent` + `DiscardRequiredEvent` per player with >7 cards; does NOT auto-remove cards — UI calls `TryRemove`; `MoveRobber` steals one random resource from victim |
| `Players/CatanPlayer.cs` | `HasPiecesRemaining` implemented — settlements (IsCity=false) < 5, cities (IsCity=true) < 4, roads < 15; `IsSecondSetupRound` used by GameManager to grant resources |
| `Cards/KnightCard.cs` | `OnPlay` calls `TurnManager.BeginKnightRobberPhase()` before `RobberSystem.Activate` |
| `Build/Road.cs` | Implements both `IPlaceable` AND `IBuildLocation` (required for `BuildManager.TryPlace` second arg) |
| `Board/CatanHexTile.cs` | `ProduceResources` casts `IPlayer` → `CatanPlayer` before accessing `.Resources` |
| `AssemblyInfo.cs` | `[assembly: InternalsVisibleTo("com.catan.tests")]` |

### Editor — `Assets/Catan/Editor/`

| File | Notes |
|---|---|
| `DiscardUICreator.cs` | Menu **Catan → Create All UI Prefabs** generates `DiscardCard.prefab`, `DiscardPanel.prefab`, `BankTradePanel.prefab` in `Assets/Catan/Prefabs/` |
| `Catan.Editor.asmdef` | Editor-only; references `Catan` + `Unity.TextMeshPro` |

---

## Scene hierarchy

```
GameManager                  ← GameManager + CatanTurnManager + BuildManager + TradeManager + ScoreManager
  BoardRenderer              ← BoardRenderer component
  RobberIcon                 ← RobberView component

Canvas (Screen Space Overlay)
  TurnPanel                  ← TurnIndicatorView
  ActionPanel                ← ActionButtonsView + 8 buttons
  Player0Panel               ← PlayerHandView (PlayerIndex=0)
  Player1Panel               ← PlayerHandView (PlayerIndex=1)
  VictoryScreen              ← VictoryScreenView (inactive by default)
  DiscardPanel               ← DiscardPanelView (inactive by default)
  BankTradePanel             ← BankTradePanelView (inactive by default)

EventSystem
```

### Inspector wires

**GameManager object:**
- TurnManager, BuildManager, TradeManager, ScoreManager → same GameObject
- BoardRenderer → BoardRenderer child
- RobberView → RobberIcon child
- WoodResource … OreResource → 5 CatanResource ScriptableObjects
- PlayerConfigs → 2–4 entries (name + color)

**DiscardPanel:**
- CardPrefab → `DiscardCard.prefab`
- ConfirmButton.OnClick → `DiscardPanelView.OnConfirmDiscard()`

**BankTradePanel:**
- ConfirmButton.OnClick → `BankTradePanelView.OnConfirm()`
- CancelButton.OnClick → `BankTradePanelView.Close()`

**ActionPanel:**
- BankTradeButton.OnClick → `ActionButtonsView.OnBankTrade()`
- ActionButtonsView.BankTradePanel → BankTradePanel scene object

**Prefabs:**
- HexTile → Collider2D (Polygon or Box)
- Vertex → CircleCollider2D
- Edge → BoxCollider2D; rotation set by BoardRenderer at spawn
- `RobberView.HexSize` must match `BoardRenderer.HexSize`

---

## Turn phase flow

```
SetupPlacement
  → each player places settlement then road (snake draft: forward then reverse)
  → 2nd round settlement placement grants adjacent tile resources
RollDice
  → 7  : Robber phase → player clicks tile → Building
  → 2-6 or 8-12: distribute resources → Trading
Trading        (can build here; End Turn available)
Building       (End Turn available)
EndTurn        → NextTurn() → RollDice
```

**Setup detail:** `GameManager.SetupSettlementPlaced` flag is set BEFORE `TryPlace` (EventBus is synchronous). On success, mode auto-switches to Road. After road is placed, `TurnManager.NextTurn()` is called automatically.

**Knight card:** switches phase to Robber immediately (enabling tile clicks), then after robber is moved returns to the phase the player was in before.

---

## Remaining work

### Not yet implemented
- **Steal target selection UI** — `TryMoveRobber` always steals from `playersOnTile[0]`; with 3-4 players and multiple enemies on the destination tile there is no way to choose the victim. Needs a small popup panel.
- **3-4 player hand panels** — turn logic and setup handle any player count correctly, but the scene only has `PlayerHandView` for players 0 and 1. Players 2 and 3 have no visible hand.
- **Longest road / largest army labels** — tracked and scoring correctly internally; no UI indicator.
- **Dev card same-turn restriction UI** — can't play a card bought this turn (logic enforced); no visual feedback explaining why the button doesn't work.
- **Player-to-player trading UI** — `TradeManager` handles it in code; no propose/accept/reject UI built yet.
- **Victory screen end-to-end test** — `VictoryScreenView` exists but has not been tested.

### Known bugs
- **City upgrade rollback** — `settlement.UpgradeToCity()` is called before `BuildManager.TryPlace`; if TryPlace fails the settlement is permanently upgraded with no road.

---

## Bug fix history

| # | Bug | Fix |
|---|---|---|
| 1 | `BuildSucceededEvent` fires synchronously inside TryPlace — UI sees stale state | Set all state BEFORE calling TryPlace |
| 2 | Border vertex positions wrong (centroid of AdjacentTiles fails for edge vertices) | `CornerWorldPosition(coord, i)` using `center + (cos(-60i°), sin(-60i°)) * HexSize` |
| 3 | All edges spawned at 0° rotation | Edge i rotated `−60*(i+1)°` by BoardRenderer |
| 4 | `Road` not accepted as `IBuildLocation` by `BuildManager.TryPlace` | `Road` now implements both `IPlaceable` and `IBuildLocation` |
| 5 | `IPlayer.Resources` doesn't exist | Cast to `CatanPlayer` in `CatanHexTile.ProduceResources` |
| 6 | `CardHand<T>.Count` doesn't exist | Use `.Cards.Count` |
| 7 | `BoardRenderer.Start()` ran before `GameManager.Start()` — board empty | Removed `Start()` from BoardRenderer; GameManager calls `RenderBoard()` explicitly |
| 8 | End Turn disabled during Trading phase | `EndTurn()` and button gate now allow Trading, Building, EndTurn |
| 9 | Building disabled during Trading phase | `canBuild` includes `isTradingPhase` |
| 10 | `DiscardPanelView` never subscribed to `DiscardRequiredEvent` | `Awake` calls `SetActive(false)` before `OnEnable` — moved to `Awake`+`OnDestroy` |
| 11 | `BankTradePanelView.AllResources` captured nulls at class load | `CatanResources` not yet initialised when `static readonly` field evaluated — changed to property |
| 12 | Knight card played but robber never moveable | `KnightCard.OnPlay` now calls `TurnManager.BeginKnightRobberPhase()` to switch phase and remember where to return |
| 13 | 2nd setup settlement gave no resources | `CatanTurnManager.IsSecondSetupRound` added; `GameManager.GrantAdjacentResources` called on 2nd-round placement |
| 14 | `HasPiecesRemaining` threw `NotImplementedException` | Implemented with per-type counts vs MaxSettlements/MaxCities/MaxRoads |
