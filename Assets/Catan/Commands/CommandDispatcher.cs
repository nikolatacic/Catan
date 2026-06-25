using UnityEngine;

namespace Catan.Commands
{
    // ── Single entry point for all player-intent commands ─────────────────────
    // Hotseat    → execute locally
    // Host       → execute locally (host IS the authority)
    // Client     → Phase 5b: serialize + ServerRpc to host
    //              Phase 5a (now): no-op + warning
    // ──────────────────────────────────────────────────────────────────────────

    public static class CommandDispatcher
    {
        public static void Send(IGameCommand command)
        {
            if (command == null) return;
            var manager = UI.GameManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("[CommandDispatcher] No GameManager available.");
                return;
            }

            if (NetworkSession.IsClient)
            {
                Debug.LogWarning(
                    $"[CommandDispatcher] {command.GetType().Name} from a client is " +
                    "not yet routed over the network. Phase 5b will add RPC routing.");
                return;
            }

            // Hotseat or Host — execute on this device.
            command.Execute(manager);
        }
    }
}
