using UnityEngine;
using UnityEngine.SceneManagement;

namespace Catan.UI
{
    public class MainMenuView : MonoBehaviour
    {
        // Scene index 1 in Build Settings = the game scene (MainScene).
        // Populates GameSession with a default 2-player hotseat config so the
        // game scene knows who's playing. Later this will be replaced with a
        // lobby/player-setup screen that writes real values into GameSession.
        public void OnPlay()
        {
            GameSession.SetDefault2PlayerHotseat();
            SceneManager.LoadScene(1);
        }
    }
}
