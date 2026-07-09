using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

namespace Catan.UI
{
    // ── Scene setup ────────────────────────────────────────────────────────────
    // Attach to the same GameObject as a UIDocument whose Visual Tree Asset is
    // Lobby.uxml. LobbyRoot starts hidden (display:none). MainMenuView calls
    // ShowAsHost() / ShowAsClient() to reveal it.
    //
    // A NetworkManager GameObject must exist in the scene. Its UnityTransport
    // component is reconfigured at runtime when a Relay allocation lands.
    // ──────────────────────────────────────────────────────────────────────────

    [RequireComponent(typeof(UIDocument))]
    public class LobbyView : MonoBehaviour
    {
        [Tooltip("Max clients besides the host. Hotseat caps at 4 players total.")]
        public int MaxClients = 3;

        private VisualElement _lobbyRoot;
        private VisualElement _hostSection;
        private VisualElement _clientSection;
        private Label _joinCodeDisplay;
        private Label _statusLabel;
        private TextField _joinCodeInput;
        private Button _confirmButton;
        private Button _backButton;

        private bool _isHostMode;
        private bool _isBusy;
        private bool _isHostAllocated;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            _lobbyRoot      = root.Q<VisualElement>("LobbyRoot");
            _hostSection    = root.Q<VisualElement>("HostSection");
            _clientSection  = root.Q<VisualElement>("ClientSection");
            _joinCodeDisplay = root.Q<Label>("JoinCodeDisplay");
            _statusLabel    = root.Q<Label>("StatusLabel");
            _joinCodeInput  = root.Q<TextField>("JoinCodeInput");
            _confirmButton  = root.Q<Button>("ConfirmButton");
            _backButton     = root.Q<Button>("BackButton");

            _confirmButton.RegisterCallback<ClickEvent>(_ => OnConfirm());
            _backButton.RegisterCallback<ClickEvent>(_ => OnBack());
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void ShowAsHost()
        {
            _isHostMode = true;
            _isHostAllocated = false;
            Show();
            SetVisible(_hostSection, true);
            SetVisible(_clientSection, false);
            if (_joinCodeDisplay != null) _joinCodeDisplay.text = "—";
            SetStatus("Click Create Room to get a join code.");
            SetConfirmText("Create Room");
        }

        public void ShowAsClient()
        {
            _isHostMode = false;
            Show();
            SetVisible(_hostSection, false);
            SetVisible(_clientSection, true);
            if (_joinCodeInput != null) _joinCodeInput.value = "";
            SetStatus("Enter join code and press Connect.");
            SetConfirmText("Connect");
        }

        // ── Button handlers ────────────────────────────────────────────────────

        private async void OnConfirm()
        {
            if (_isBusy) return;
            _isBusy = true;
            SetConfirmInteractable(false);

            try
            {
                if (_isHostMode) await StartAsHostAsync();
                else             await StartAsClientAsync();
            }
            catch (Exception exception)
            {
                SetStatus($"Failed: {exception.Message}");
                Debug.LogException(exception);
                _isBusy = false;
                SetConfirmInteractable(true);
            }
        }

        private void OnBack()
        {
            if (_isBusy) return;
            SetVisible(_lobbyRoot, false);
            _isHostAllocated = false;
            NetworkSession.EnterHotseatMode();
        }

        // ── Host flow ──────────────────────────────────────────────────────────

        // Two-stage: first click allocates relay and shows code; second loads game.
        private async Task StartAsHostAsync()
        {
            if (!_isHostAllocated)
            {
                SetStatus("Signing in to Unity Services…");
                await EnsureSignedInAsync();

                SetStatus("Allocating relay…");
                var allocation = await RelayService.Instance.CreateAllocationAsync(MaxClients);

                SetStatus("Requesting join code…");
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                if (_joinCodeDisplay != null) _joinCodeDisplay.text = joinCode;

                ConfigureTransportForHost(allocation);

                NetworkSession.EnterHostMode(joinCode);

                SetStatus("Starting host…");
                if (!NetworkManager.Singleton.StartHost())
                    throw new InvalidOperationException("NetworkManager.StartHost returned false.");

                _isHostAllocated = true;
                SetStatus("Room is open — share the code, then click Start Game when ready.");
                SetConfirmText("Start Game");
                _isBusy = false;
                SetConfirmInteractable(true);
                return;
            }

            SetStatus("Loading game…");
            int connectedPlayerCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
            GameSession.SetNetworkedPlayers(connectedPlayerCount);
            NetworkManager.Singleton.SceneManager.LoadScene("GameHotseat", LoadSceneMode.Single);
        }

        // ── Client flow ────────────────────────────────────────────────────────

        private async Task StartAsClientAsync()
        {
            var joinCode = _joinCodeInput?.value?.Trim();
            if (string.IsNullOrEmpty(joinCode))
            {
                SetStatus("Enter a join code.");
                _isBusy = false;
                SetConfirmInteractable(true);
                return;
            }

            SetStatus("Signing in to Unity Services…");
            await EnsureSignedInAsync();

            SetStatus($"Joining {joinCode}…");
            var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            ConfigureTransportForClient(allocation);

            NetworkSession.EnterClientMode(joinCode);

            SetStatus("Connecting to host…");
            if (!NetworkManager.Singleton.StartClient())
                throw new InvalidOperationException("NetworkManager.StartClient returned false.");
            // Host loads GameHotseat via NetworkManager.SceneManager; client follows automatically.
        }

        // ── Unity Services helpers ─────────────────────────────────────────────

        private static async Task EnsureSignedInAsync()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private static void ConfigureTransportForHost(Allocation allocation)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new Unity.Networking.Transport.Relay.RelayServerData(
                allocation, "dtls"));
        }

        private static void ConfigureTransportForClient(JoinAllocation allocation)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new Unity.Networking.Transport.Relay.RelayServerData(
                allocation, "dtls"));
        }

        // ── UI helpers ─────────────────────────────────────────────────────────

        private void Show()
        {
            _isBusy = false;
            SetVisible(_lobbyRoot, true);
            SetConfirmInteractable(true);
        }

        private void SetStatus(string text)
        {
            if (_statusLabel != null) _statusLabel.text = text;
        }

        private void SetConfirmText(string text)
        {
            if (_confirmButton != null) _confirmButton.text = text;
        }

        private void SetConfirmInteractable(bool interactable)
        {
            if (_confirmButton != null) _confirmButton.SetEnabled(interactable);
        }

        private static void SetVisible(VisualElement element, bool visible)
        {
            if (element != null)
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
