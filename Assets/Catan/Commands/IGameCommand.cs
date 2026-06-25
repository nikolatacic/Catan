using Catan.Network;

namespace Catan.Commands
{
    // ── Command pattern: player intents that may cross the network ────────────
    // Two methods:
    //   Execute(manager)        — runs on the host (or hotseat) authority
    //   SendOverNetwork(bridge) — client serializes + invokes the bridge's
    //                             ServerRpc; ignored on host/hotseat
    // ──────────────────────────────────────────────────────────────────────────

    public interface IGameCommand
    {
        void Execute(UI.GameManager manager);
        void SendOverNetwork(NetworkCommandBridge bridge);
    }
}
