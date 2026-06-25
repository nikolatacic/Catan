# Multiplayer Plan

Target: 4 friends on iOS/Android, same game session, minimal setup effort.  
Stack: **Unity Relay + Netcode for GameObjects (NGO)**, host-client model (one player's device is the authority).

---

## Phase 1 — Scene separation ✅ DONE

- `MainMenu` scene: logo + Play button. Will later become the root for multiplayer lobby.
- `MainScene` (the existing game scene): unchanged. Can be renamed to `GameHotseat` later.
- Build Settings order: MainMenu (index 0), MainScene (index 1).
- Play button → `SceneManager.LoadScene(1)` (index-based so renaming the game scene is safe).

Editor tool: `Catan → Create Main Menu Scene` regenerates the scene if needed.

## Phase 2 — Decouple player configuration from the scene ✅ DONE

`GameSession` (static class, `Assets/Catan/Session/GameSession.cs`) holds the
player list across scene loads. `MainMenuView.OnPlay` populates it with
`SetDefault2PlayerHotseat()` before loading the game scene.

`GameManager.CreatePlayers` reads from `GameSession` when populated; otherwise
falls back to the Inspector `PlayerConfigs` list. The game scene therefore
still runs standalone for testing.

This is the seam where the future lobby screen and (later) the multiplayer
connection handshake will write player configs. The game scene doesn't care
where the data came from.

## Phase 3 — Identify the network boundary ✅ DONE

Full audit lives in [`NetworkBoundaryAudit.md`](NetworkBoundaryAudit.md). Summary:

- **Authority** (host-only): `CatanBoard`, `CatanPlayer.*`, `CatanTurnManager`,
  `RobberSystem`, `DiceManager`, `ScoreManager`, dev-card deck, all rules
- **Commands** (client → host RPC): every `Try*` method on `GameManager`
  (13 total)
- **Local-only**: `Begin*Placement`/`CancelPlacement` (UI-mode toggles)
- **Events**: 18 events catalogued; flagged 3 that leak hidden info
  (`ResourceProducedEvent`, `ResourceAddedEvent`, `ResourceStolenEvent`) and
  must be redacted for non-recipients in multiplayer
- **Refactors needed before networking**: `Settlement.UpgradeToCity()` is
  mutation-before-validation (hotseat hack); `CheatMenuView` mutates directly;
  `EventBus.Publish` needs a network-aware wrapper; dice RNG must run on host

## Phase 4 — Command pattern for game actions ✅ DONE

`IGameCommand` interface + `CommandDispatcher` (static) + 10 concrete commands
in `Assets/Catan/Commands/`. Every UI caller that used to invoke
`GameManager.Instance.Try*` now dispatches a command instead.

In hotseat the dispatcher executes locally (zero behaviour change). In Phase 5
it branches: host executes directly, client serializes and sends a ServerRpc
whose handler executes the same command on the host.

`Begin*Placement` / `CancelPlacement` remain direct calls — local UI-mode
toggles that don't need to cross the network.

One forward-compatible adjustment: `BankTradePanelView.OnConfirm` now detects
success via a before/after resource snapshot instead of a synchronous return
value — this works identically in hotseat and survives async RPC handling.

## Phase 5 — Unity Relay + NGO integration

Split into three sub-phases because the work is too large for a single session.

### Phase 5a — Lobby scaffolding ✅ DONE

- Added packages: `com.unity.netcode.gameobjects`, `com.unity.transport`,
  `com.unity.services.core`, `com.unity.services.authentication`, `com.unity.services.relay`
- `NetworkSession` (static) — mirrors `GameSession` for connection state
  (`Hotseat` / `Host` / `Client`, `JoinCode`, `LocalPlayerId`)
- `LobbyView` — Relay create/join + StartHost/StartClient + scene load
- `MainMenuView` — added `OnHost` / `OnJoin` alongside existing `OnPlay`
- `CommandDispatcher` is now NetworkSession-aware (clients no-op + warn until 5b)
- Editor tool: `Catan → Add Multiplayer Lobby to Main Menu` adds NetworkManager
  + UnityTransport + a wired LobbyPanel to the existing MainMenu scene

Manual one-time Unity setup:
1. Let Unity import the new packages on first open
2. Edit → Project Settings → Services → link this project to a Unity Cloud
   project (required for Relay + Authentication)
3. Run `Catan → Add Multiplayer Lobby to Main Menu`

After 5a: hosting / joining works end-to-end at the Relay/transport level —
peers connect, scenes load together via NGO's NetworkSceneManager. But game
state changes from clients don't reach the host yet.

### Phase 5b — Command routing through RPCs ✅ DONE

- `NetworkCommandBridge` (in-scene `NetworkBehaviour`) — sibling to GameManager.
  10 `ServerRpc` methods, one per command.
- Vertices / edges encoded as `(HexCoord tile, byte index 0-5)` — topology is
  deterministic from board size so the same key resolves to the same object on
  every peer. Tile *contents* still differ until 5c lands.
- `IGameCommand` extended with `SendOverNetwork(bridge)`; each command serializes
  its own payload and invokes the matching ServerRpc.
- `CommandDispatcher` routes through the bridge when `IsClient`.
- Editor tool: `Catan → Add Network Command Bridge to MainScene`.

What still needs 5c:
- Host-side event fan-out via `ClientRpc` (UI subscribers are passive and need
  no code changes — they just need the events to fire)
- Three flagged events get redacted per-client:
  `ResourceProducedEvent`, `ResourceAddedEvent`, `ResourceStolenEvent`
- `DiceManager` RNG runs only on the host; `DiceRolledEvent` fans out

### Phase 5c — Seed sync + EventBus fan-out ✅ DONE

- `GameManager` defers full initialization on client until the host's seed
  arrives via `NetworkEventBridge`. Both peers then generate identical boards
  from the same seed.
- `NetworkEventBridge` (in-scene `NetworkBehaviour`, sibling of
  `NetworkCommandBridge`): subscribes to 12 host events and re-emits each via
  `ClientRpc`. Clients publish locally so the existing UI subscribers react.
- `BuildSucceeded` ClientRpc mirrors the host's board mutation on every client
  so subsequent rule evaluations match.
- Editor tool now adds both bridges to GameHotseat in one click.

### Phase 5d — Per-device UI + index assignment ✅ DONE (partial)

- `NetworkSession.LocalPlayerIndex` — set by host when a peer connects.
  Host = 0, first joiner = 1, etc.
- `NetworkEventBridge.AssignPlayerIndexClientRpc` (targeted) tells each peer
  their index; publishes `LocalPlayerAssignedEvent` on receipt.
- `PlayerHandView` / `DevHandView`: in hotseat follow active player as before;
  in networked mode always show the local player's hand.
- `ActionButtonsView`: disables every button when networked and the local
  player is not the active player. Host plays normally on its own turn;
  clients see disabled buttons until it's their turn.
- Late-joiner seed broadcast: targeted seed RPC sent at index-assignment
  time so a late client builds its board before any other event lands.

### Phase 5e — Hidden state + lobby player picker 👈 NEXT

- **Hidden state filtering**: redact `ResourceAdded` / `ResourceRemoved` /
  `ResourceStolen` for non-owners — they see only count deltas, not which
  resource. Same for `DevCardPurchasedEvent`.
- **Real lobby player picker**: replaces `SetDefault2PlayerHotseat()`. Host
  configures players in the lobby; sends the roster to clients on join.
- **StealTargetPanel for 3+ players**: currently the panel only opens on the
  host. Active player on any device should see it.
- **`DiceManager` host-only**: guard with `IsServer` for safety (currently
  only reached via host-side command path, but worth defense-in-depth).

## Phase 6 — Per-device UI

- Hotseat: all UI on one screen, `PlayerHandView` follows active player.
- Multiplayer: each device shows its own hand (`localPlayer` instead of `activePlayer`).
- Public info (board, scores, other players' card counts) synced to everyone.
- `PlayerHandView` and `DevHandView` switch to a `localPlayer` reference set at connection time.

---

## Recommended order

| Step | Phase | Effort |
|---|---|---|
| ✅ Done | 1 — MainMenu scene + Play button | ~20 min |
| ✅ Done | 2 — GameSession data carrier | ~1 session |
| ✅ Done | 3 — Boundary audit (no code) | ~1 session |
| ✅ Done | 4 — Command pattern | ~1 session |
| ✅ Done | 5a — Lobby scaffolding | ~1 session |
| ✅ Done | 5b — Command routing through RPCs | ~1 session |
| ✅ Done | 5c — Seed sync + EventBus fan-out | ~1 session |
| ✅ Done | 5d — Per-device UI + index assignment | ~1 session |
| 👈 Next | 5e — Hidden state + lobby player picker | ~1 session |
| Polish  | 6 — Final polish / multiplayer edge cases | ~1 session |

---

## Notes

- Keep hotseat scene working throughout — it's the offline fallback and test bed.
- All core logic (`CatanTurnManager`, build rules, score, resources) is pure C# with no Unity dependencies. This makes it straightforward to keep on the host side as authoritative logic without tangling into network code.
- Don't retrofit multiplayer into the existing scene — build the networked version alongside it.
