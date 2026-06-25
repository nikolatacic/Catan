using UnityEngine;

namespace Catan.Commands
{
    // ── Single entry point for all player-intent commands ─────────────────────
    // Today: executes locally (hotseat).
    // Phase 5: branches on IsNetworked → host executes directly, clients send
    //   a ServerRpc whose handler calls command.Execute on the host instance.
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
            command.Execute(manager);
        }
    }
}
