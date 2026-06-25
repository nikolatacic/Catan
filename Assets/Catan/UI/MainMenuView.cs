using UnityEngine;
using UnityEngine.SceneManagement;

namespace Catan.UI
{
    public class MainMenuView : MonoBehaviour
    {
        // Scene index 1 in Build Settings = GameHotseat.
        // Change to SceneManager.LoadScene("GameHotseat") once you rename the scene.
        public void OnPlay() => SceneManager.LoadScene(1);
    }
}
