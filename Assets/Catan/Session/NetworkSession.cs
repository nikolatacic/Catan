namespace Catan
{
    // ── Cross-scene network/connection state ───────────────────────────────────
    // Static so it survives SceneManager.LoadScene without needing
    // DontDestroyOnLoad on a GameObject. Parallels GameSession for connection
    // metadata (host vs client, join code, local player id, etc.).
    //
    // Phase 5a: just a state holder. The CommandDispatcher (and later, the
    // network layer) reads from here to decide whether to execute locally
    // or route over the network.
    // ──────────────────────────────────────────────────────────────────────────

    public enum NetworkMode
    {
        Hotseat,        // No network. CommandDispatcher executes locally.
        Host,           // This device is the authoritative host + a player.
        Client,         // This device is a client; commands go to host as RPCs.
    }

    public static class NetworkSession
    {
        public static NetworkMode Mode { get; private set; } = NetworkMode.Hotseat;
        public static string JoinCode { get; private set; }
        public static ulong LocalPlayerId { get; private set; }

        public static bool IsNetworked => Mode != NetworkMode.Hotseat;
        public static bool IsHost      => Mode == NetworkMode.Host;
        public static bool IsClient    => Mode == NetworkMode.Client;

        public static void EnterHotseatMode()
        {
            Mode     = NetworkMode.Hotseat;
            JoinCode = null;
            LocalPlayerId = 0;
        }

        public static void EnterHostMode(string joinCode)
        {
            Mode     = NetworkMode.Host;
            JoinCode = joinCode;
        }

        public static void EnterClientMode(string joinCode)
        {
            Mode     = NetworkMode.Client;
            JoinCode = joinCode;
        }

        public static void SetLocalPlayerId(ulong id)
        {
            LocalPlayerId = id;
        }
    }
}
