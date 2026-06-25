using UnityEngine;
using Catan.Network;

namespace Catan.Commands
{
    // ── Single entry point for all player-intent commands ─────────────────────
    // Hotseat → execute locally
    // Host    → execute locally (host IS the authority)
    // Client  → SendOverNetwork → bridge ServerRpc → host executes
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
                var bridge = NetworkCommandBridge.Instance;
                if (bridge == null)
                {
                    Debug.LogWarning(
                        "[CommandDispatcher] Client tried to send a command but the " +
                        "NetworkCommandBridge isn't spawned yet.");
                    return;
                }
                command.SendOverNetwork(bridge);
                return;
            }

            // Hotseat or Host — execute on this device.
            command.Execute(manager);
        }
    }
}
