using UnityEngine;
using UnityEngine.SceneManagement;

namespace Catan.UI
{
    public class MainMenuView : MonoBehaviour
    {
        [Header("Lobby (optional — required for Host/Join)")]
        public LobbyView Lobby;

        // ── Play (hotseat) ─────────────────────────────────────────────────────
        // Scene index 1 in Build Settings = the game scene (MainScene).
        // Populates GameSession with a default 2-player hotseat config so the
        // game scene knows who's playing.
        public void OnPlay()
        {
            NetworkSession.EnterHotseatMode();
            GameSession.SetDefault2PlayerHotseat();
            SceneManager.LoadScene(1);
        }

        // ── Host (multiplayer) ─────────────────────────────────────────────────
        public void OnHost()
        {
            if (Lobby != null) Lobby.ShowAsHost();
        }

        // ── Join (multiplayer) ─────────────────────────────────────────────────
        public void OnJoin()
        {
            if (Lobby != null) Lobby.ShowAsClient();
        }
    }
}
