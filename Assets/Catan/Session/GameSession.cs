using System.Collections.Generic;
using UnityEngine;

namespace Catan
{
    // ── Cross-scene session carrier ────────────────────────────────────────────
    // Holds the player list (and later: lobby/network info) between MainMenu
    // and the game scene. Static so it survives SceneManager.LoadScene without
    // needing DontDestroyOnLoad on a GameObject.
    //
    // Phase 2 of MultiplayerPlan: this is the seam where a future lobby screen
    // will write player configs that the game scene reads on Start.
    // ──────────────────────────────────────────────────────────────────────────

    public static class GameSession
    {
        // True once a menu/lobby has populated the player list.
        // When false, GameManager falls back to its Inspector-serialized
        // PlayerConfigs so the game scene still runs standalone for testing.
        public static bool HasPlayers => _players.Count > 0;

        private static readonly List<UI.PlayerConfig> _players = new();

        public static IReadOnlyList<UI.PlayerConfig> Players => _players;

        public static void SetPlayers(IEnumerable<UI.PlayerConfig> players)
        {
            _players.Clear();
            foreach (var player in players)
                _players.Add(player);
        }

        public static void Clear() => _players.Clear();

        private static readonly Color[] DefaultPlayerColors =
        {
            new Color(0.85f, 0.20f, 0.20f), // red
            new Color(0.20f, 0.40f, 0.85f), // blue
            new Color(0.20f, 0.70f, 0.25f), // green
            new Color(0.90f, 0.75f, 0.10f), // yellow
        };

        // ── Default 2-player setup used by MainMenu's quick-play button ────────
        public static void SetDefault2PlayerHotseat()
        {
            SetPlayers(new[]
            {
                new UI.PlayerConfig { PlayerName = "Player 1", PlayerColor = DefaultPlayerColors[0] },
                new UI.PlayerConfig { PlayerName = "Player 2", PlayerColor = DefaultPlayerColors[1] },
            });
        }

        // ── Networked game: creates N players with default names and colors ────
        public static void SetNetworkedPlayers(int count)
        {
            int clampedCount = Mathf.Clamp(count, 2, DefaultPlayerColors.Length);
            var configs = new UI.PlayerConfig[clampedCount];
            for (int playerIndex = 0; playerIndex < clampedCount; playerIndex++)
            {
                configs[playerIndex] = new UI.PlayerConfig
                {
                    PlayerName = $"Player {playerIndex + 1}",
                    PlayerColor = DefaultPlayerColors[playerIndex],
                };
            }
            SetPlayers(configs);
        }
    }
}
