# Network Boundary Audit (Phase 3)

Goal: classify every system in the codebase as **Authority** (host-only),
**Replica** (read-only on every client), or **Command** (client → host RPC).
This is the input to Phase 4 (command pattern) and Phase 5 (Relay + NGO).

No code changes here — purely a reading exercise. When a system crosses the
boundary in non-obvious ways, that's noted explicitly.

---

## Legend

| Tag | Meaning |
|---|---|
| 🟥 **Authority** | Only the host runs this. Mutates game state. |
| 🟦 **Replica** | Runs on every client. Reads state, renders UI, never mutates. |
| 🟨 **Command** | Player intent that crosses the network. Becomes a ServerRpc. |
| 🟩 **Event** | State change broadcast by the host to all clients. |
| ⚪ **Local-only** | Cosmetic/local state. Same on all peers, no sync. |

---

## Game state — what lives on the host

These objects ARE the game. The host owns them; clients see snapshots.

| System | File | Why authority |
|---|---|---|
| 🟥 `CatanBoard` (tiles, settlements dict, roads dict, ports, robber pos) | `Board/CatanBoard.cs` | Single source of truth for the board |
| 🟥 `CatanPlayer.Resources` (ResourceInventory) | `Players/CatanPlayer.cs` | Resource counts are server-authoritative |
| 🟥 `CatanPlayer.Settlements`, `.Roads` | `Players/CatanPlayer.cs` | Owned pieces |
| 🟥 `CatanPlayer.DevelopmentCards` | `Players/CatanPlayer.cs` | Card hand (hidden from other clients) |
| 🟥 `CatanPlayer.KnightsPlayed`, `HasLargestArmy`, `HasLongestRoad` | `Players/CatanPlayer.cs` | Derived from authoritative state |
| 🟥 `CatanTurnManager` (current actor, phase, turn number, setup tracking) | `Turn/CatanTurnManager.cs` | Whose turn + what phase = host decides |
| 🟥 `RobberSystem` | `Turn/RobberSystem.cs` | Steal logic must be authoritative |
| 🟥 `DiceManager` | `Turn/DiceManager.cs` | Roll RNG must be authoritative (anti-cheat) |
| 🟥 `ScoreManager` + `CatanVictoryCondition` | `Score/` | Win detection runs on host |
| 🟥 `LongestRoadTracker`, `LargestArmyTracker` | `Score/` | Recalc lives with host |
| 🟥 `CardDeck<DevelopmentCard>` (the draw pile) | inside `GameManager` | Card order is hidden state |
| 🟥 `CatanBuildRule`, `CatanTradeRule` | `Rules/` | Validation logic runs only on host |

**Hidden state** (clients should NOT see other players' contents):
- `CatanPlayer.Resources` — only count is public; specific composition is private to the owner
- `CatanPlayer.DevelopmentCards` — same
- `CardDeck<DevelopmentCard>` — entirely hidden

---

## Command surface — `GameManager` public methods

Every method below is a **player intent**. In hotseat each call mutates state
directly. In multiplayer each becomes a `ServerRpc` from the player who clicked
to the host, where the same logic runs.

| Method | File:Line | Validation today | Hotseat caller |
|---|---|---|---|
| 🟨 `RequestRoll()` | `GameManager.cs:175` | Phase==RollDice | `ActionButtonsView.OnRollDice` |
| 🟨 `EndTurn()` | `GameManager.cs:181` | Phase ∈ {Building, EndTurn, Trading} | `ActionButtonsView.OnEndTurn` |
| 🟨 `BeginPlaceSettlement()` | `GameManager.cs:192` | None (UI state only) | `ActionButtonsView.OnBuildSettlement` |
| 🟨 `BeginPlaceRoad()` | `GameManager.cs:193` | None (UI state only) | `ActionButtonsView.OnBuildRoad` |
| 🟨 `BeginUpgradeCity()` | `GameManager.cs:194` | None (UI state only) | `ActionButtonsView.OnBuildCity` |
| 🟨 `CancelPlacement()` | `GameManager.cs:195` | None | `ActionButtonsView.OnCancelPlacement` |
| 🟨 `TryPlaceSettlement(vertex)` | `GameManager.cs:197` | `CatanBuildRule.CanPlace` | `VertexView.OnMouseDown` |
| 🟨 `TryPlaceRoad(edge)` | `GameManager.cs:233` | `CatanBuildRule.CanPlace` | `EdgeView.OnMouseDown` |
| 🟨 `TryUpgradeCity(vertex)` | `GameManager.cs:260` | `CatanBuildRule.CanPlace` | `VertexView.OnMouseDown` |
| 🟨 `TryMoveRobber(coord)` | `GameManager.cs:280` | Phase==Robber | `HexTileView.OnMouseDown` |
| 🟨 `CompleteRobberMove(coord, victim)` | `GameManager.cs:297` | (no extra check) | `StealTargetPanelView.OnVictimSelected` |
| 🟨 `TryPurchaseDevCard()` | `GameManager.cs:304` | Phase + cost | `ActionButtonsView.OnBuyDevCard` |
| 🟨 `TryBankTrade(give, receive)` | `GameManager.cs:350` | `CatanTradeRule` + cost | `BankTradePanelView.OnConfirm` |
| 🟨 `TryPlayDevCard(card)` | `GameManager.cs:377` | `card.IsPlayable` + once-per-turn | `DevCardItemView.OnPlayClicked` |

**Note:** `BeginPlaceSettlement/Road/City` and `CancelPlacement` are pure UI-mode
toggles for the *local* player's highlight system. They do NOT need to go over
the network — they should stay client-local. The real network call only happens
on the subsequent `TryPlace…` when the player clicks a vertex/edge.

**Cheats** (`CheatMenuView.cs`): `CheatAddVP`, `CheatGiveAllResources`,
`CheatSkipToEndTurn` mutate state directly. In multiplayer these would need to
be host-only and either disabled or guarded by an "is debug build" check.

---

## Events — what the host broadcasts

All `EventBus.Publish<...>` calls. In multiplayer these need to fan out from
the host to every client as ClientRpcs (or via NetworkVariable change callbacks).

| Event | Published by | Carries | Sensitive? |
|---|---|---|---|
| 🟩 `DiceRolledEvent` | `DiceManager` | D1, D2, total | No |
| 🟩 `SevenRolledEvent` | `CatanTurnManager` | active player | No |
| 🟩 `RobberMovedEvent` | `RobberSystem` | from, to, mover | No |
| 🟩 `ResourceStolenEvent` | `RobberSystem` | thief, victim, **stolen resource** | **Yes — only thief and victim should see the resource** |
| 🟩 `DiscardRequiredEvent` | `CatanTurnManager` | player, count | No (count only) |
| 🟩 `KnightPlayedEvent` | `KnightCard.OnPlay` | player | No |
| 🟩 `DevCardPurchasedEvent` | `GameManager.TryPurchaseDevCard` | player | No (only buyer needs the card itself) |
| 🟩 `CatanPhaseChangedEvent` | `CatanTurnManager` | from, to | No |
| 🟩 `TurnStartedEvent` (gamecore) | turn manager | actor | No |
| 🟩 `ResourceProducedEvent` | `CatanHexTile.ProduceResources` | tile, list of (player, bundle) | **Partially — players should see totals, but specific resource cards belong to the recipient only** |
| 🟩 `ResourceAddedEvent` / `ResourceRemovedEvent` (gamecore) | `ResourceInventory` | player, bundle | **Yes — only the owner should see specific cards; others see only count delta** |
| 🟩 `BuildSucceededEvent` / `BuildFailedEvent` (gamecore) | `BuildManager` | player, piece, location | No |
| 🟩 `ScoreChangedEvent` (gamecore) | `ScoreManager` | player, old, new | No |
| 🟩 `LeaderChangedEvent` (gamecore) | `ScoreManager` | previous, current | No |
| 🟩 `VictoryAchievedEvent` (gamecore) | `ScoreManager` | winner | No |
| 🟩 `CardPlayedEvent<T>` (gamecore) | `CardHand.Play` | card | No |
| 🟩 `PlacementModeChangedEvent` | `GameManager` setter | mode | ⚪ Local-only — should NOT cross network |

---

## Replicas — UI and rendering

These run on every client. They read host state via the event bus + game-state
snapshots. None of them should ever mutate the authoritative model.

| Component | File | What it shows |
|---|---|---|
| 🟦 `BoardRenderer` | `UI/BoardRenderer.cs` | Spawns tile/vertex/edge views from board snapshot |
| 🟦 `HexTileView` | `UI/HexTileView.cs` | One hex; also click-source for robber move (🟨 command) |
| 🟦 `VertexView` | `UI/VertexView.cs` | Settlement/city sprite + highlight; click-source for place/upgrade |
| 🟦 `EdgeView` | `UI/EdgeView.cs` | Road sprite + highlight; click-source for road placement |
| 🟦 `RobberView` | `UI/RobberView.cs` | Robber position |
| 🟦 `PlayerHandView` | `UI/PlayerHandView.cs` | Active player's resource cards |
| 🟦 `DevHandView` | `UI/DevHandView.cs` | Active player's dev cards |
| 🟦 `AllPlayersSummaryView` | `UI/AllPlayersSummaryView.cs` | All players: VP, card count, knights, badges |
| 🟦 `PlayerSummaryRowView` | `UI/PlayerSummaryRowView.cs` | Single row in summary |
| 🟦 `ActionButtonsView` | `UI/ActionButtonsView.cs` | Roll/Build/EndTurn/etc. — gates button interactability |
| 🟦 `BankTradePanelView` | `UI/BankTradePanelView.cs` | Bank trade UI |
| 🟦 `DiscardPanelView` / `DiscardCardView` | `UI/Discard*.cs` | 7-roll discard |
| 🟦 `StealTargetPanelView` | `UI/StealTargetPanelView.cs` | Choose steal victim |
| 🟦 `TurnIndicatorView` | `UI/TurnIndicatorView.cs` | Whose turn label |
| 🟦 `VictoryScreenView` | `UI/VictoryScreenView.cs` | End-of-game overlay |
| 🟦 `CheatMenuView` | `UI/CheatMenuView.cs` | Debug only — gated in multiplayer |
| 🟦 `MainMenuView` | `UI/MainMenuView.cs` | Pre-game menu |

---

## Things that need refactoring for networking

These are spots where Phase 4 will have to do real work.

1. **`GameManager` mixes authority and UI orchestration.**
   `TryPlaceSettlement` both validates+mutates AND tells `CurrentPlacementMode`
   to update (a UI concern). The mutation half stays on host; the placement-mode
   half stays on client. Phase 4 should split these.

2. **`Settlement.UpgradeToCity()` is called before `BuildManager.TryPlace`**
   (`GameManager.cs:269`) so the rule's switch routes correctly. The rule then
   silently accepts an already-true `IsCity`. This is a hotseat-only hack that
   becomes wrong over the network — the client could pre-mutate state. Phase 4
   should route by piece-type enum, not by mutation-then-rollback.

3. **Resource events leak hidden information.**
   `ResourceProducedEvent` carries the specific resources gained. In multiplayer
   that should be: every client sees `+N cards` for non-owners, the recipient
   sees the actual cards. Same for `ResourceStolenEvent`.

4. **`CheatMenuView` mutates state without going through commands.**
   Either remove from networked builds or wrap each cheat in an
   "if (IsServer) …" check + ClientRpc broadcast.

5. **`EventBus` is process-local and synchronous.**
   In multiplayer, host-published events need to reach clients via NGO. Two paths:
   (a) Keep `EventBus` local on each peer; the network layer republishes after
   receiving a ClientRpc, OR (b) replace `EventBus.Publish` with a thin wrapper
   that routes through NGO when networked. (a) is simpler and keeps the existing
   subscriber graph intact.

6. **`DiceManager` uses `System.Random`.** Either seed it on the host and let
   clients see only `DiceRolledEvent`, or use a deterministic seeded RNG that
   all peers agree on. Path (1) is simpler.

7. **Setup phase has implicit ordering** (`_setupTurnsCompleted`). Snake order
   for setup-round 2 must be enforced on host; clients display turn order from
   `CatanTurnManager.CurrentActor` only.

---

## What's NOT a problem

- **Core logic is already pure C# with no Unity deps** (`CatanTurnManager`,
  `CatanBuildRule`, `CatanBoard`, `ScoreManager`, all of `GameCore.*`). These
  drop in as host-side code unchanged.
- **EventBus is a clean seam.** Every UI component is already a passive observer
  of state changes — they will continue to work as-is when the events come from
  the network instead of locally.
- **No Update() loops touch state.** All mutations are click-driven, which maps
  naturally to ServerRpc calls.

---

## Phase 4 follow-up

The audit feeds directly into the command layer:

1. Define a `IGameCommand` interface with one method: `Execute(GameManager)`.
2. Implement one command per 🟨 row above (`PlaceSettlementCommand`,
   `PlaceRoadCommand`, `UpgradeCityCommand`, `MoveRobberCommand`,
   `CompleteRobberMoveCommand`, `PurchaseDevCardCommand`,
   `BankTradeCommand`, `PlayDevCardCommand`, `RequestRollCommand`,
   `EndTurnCommand`).
3. Introduce a `CommandDispatcher` that executes locally in hotseat mode.
   In multiplayer mode the dispatcher serializes the command, sends a ServerRpc,
   and the host executes.
4. UI components call `CommandDispatcher.Send(new PlaceSettlementCommand(vertex))`
   instead of `GameManager.Instance.TryPlaceSettlement(vertex)`.
5. Don't wrap `Begin*Placement`/`CancelPlacement` — those stay local.
