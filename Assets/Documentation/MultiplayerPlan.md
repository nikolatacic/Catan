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

## Phase 4 — Command pattern for game actions

Current flow: UI → `GameManager.TryPlaceSettlement(vertex)` → executes immediately.  
Required flow: UI → `Command` → host validates + executes → result fans out to all clients.

Introduce lightweight command structs (`PlaceSettlementCommand`, `PlaceRoadCommand`, etc.).  
In hotseat mode they execute inline (no behaviour change).  
In networked mode they serialize and become ServerRPCs.

This is the largest architecture change and the prerequisite for Phase 5.

## Phase 5 — Unity Relay + NGO integration

- Add packages: `com.unity.netcode.gameobjects`, `com.unity.services.relay`.
- Menu gets Host / Join flow (join code copied or shown as QR for mobile ease).
- `GameManager` runs authority logic only on host (`IsServer` guard).
- Commands from Phase 4 become `ServerRpc` methods.
- Shared state (`Board`, player resources, settlements, scores) synced via `NetworkVariable` or `ClientRpc` after each state change.
- `EventBus.Publish` on host → equivalent fan-out to all clients.

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
| 👈 Next | 4 — Command pattern | ~2 sessions |
| When ready | 5 — Relay + NGO | multiple sessions |
| Polish | 6 — Per-device UI | ~1 session |

---

## Notes

- Keep hotseat scene working throughout — it's the offline fallback and test bed.
- All core logic (`CatanTurnManager`, build rules, score, resources) is pure C# with no Unity dependencies. This makes it straightforward to keep on the host side as authoritative logic without tangling into network code.
- Don't retrofit multiplayer into the existing scene — build the networked version alongside it.
