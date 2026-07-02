using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuView : MonoBehaviour
    {
        private Button _playButton;
        private Button _hostButton;
        private Button _joinButton;

        [Header("Lobby (required for Host/Join)")]
        public LobbyView Lobby;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _playButton = root.Q<Button>("PlayButton");
            _hostButton = root.Q<Button>("HostButton");
            _joinButton = root.Q<Button>("JoinButton");

            _playButton.RegisterCallback<ClickEvent>(_ => OnPlay());
            _hostButton.RegisterCallback<ClickEvent>(_ => OnHost());
            _joinButton.RegisterCallback<ClickEvent>(_ => OnJoin());
        }

        private void OnPlay()
        {
            NetworkSession.EnterHotseatMode();
            GameSession.SetDefault2PlayerHotseat();
            SceneManager.LoadScene(1);
        }

        private void OnHost()
        {
            if (Lobby != null) Lobby.ShowAsHost();
        }

        private void OnJoin()
        {
            if (Lobby != null) Lobby.ShowAsClient();
        }
    }
}
