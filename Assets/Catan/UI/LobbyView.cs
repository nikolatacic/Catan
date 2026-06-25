using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

namespace Catan.UI
{
    // ── Scene setup ────────────────────────────────────────────────────────────
    // Lives inside the MainMenu scene as a child of Canvas (starts hidden).
    // MainMenuView's "Host" / "Join" buttons call ShowAsHost() / ShowAsClient()
    // to reveal this panel.
    //
    // Required Inspector wiring:
    //   StatusLabel       — TMP, shows progress / errors
    //   JoinCodeDisplay   — TMP, shows the code to share (host mode)
    //   JoinCodeInput     — TMP_InputField, accepts code (client mode)
    //   ConfirmButton     — Button, "Start Game" (host) or "Connect" (client)
    //   BackButton        — Button, returns to main menu
    //
    // A NetworkManager GameObject must exist in the scene (added by the
    // Catan → Create Main Menu Scene editor tool). Its UnityTransport
    // component is reconfigured at runtime when a Relay allocation lands.
    // ──────────────────────────────────────────────────────────────────────────

    public class LobbyView : MonoBehaviour
    {
        [Header("Common")]
        public GameObject Root;
        public TextMeshProUGUI StatusLabel;
        public Button ConfirmButton;
        public Button BackButton;

        [Header("Host mode")]
        public GameObject HostSection;
        public TextMeshProUGUI JoinCodeDisplay;

        [Header("Client mode")]
        public GameObject ClientSection;
        public TMP_InputField JoinCodeInput;

        [Header("Limits")]
        [Tooltip("Max number of clients besides the host. Hotseat caps at 4 players total.")]
        public int MaxClients = 3;

        private bool _isHostMode;
        private bool _isBusy;

        private void Awake()
        {
            if (Root != null) Root.SetActive(false);
        }

        private void OnEnable()
        {
            if (ConfirmButton != null) ConfirmButton.onClick.AddListener(OnConfirm);
            if (BackButton != null)    BackButton.onClick.AddListener(OnBack);
        }

        private void OnDisable()
        {
            if (ConfirmButton != null) ConfirmButton.onClick.RemoveListener(OnConfirm);
            if (BackButton != null)    BackButton.onClick.RemoveListener(OnBack);
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void ShowAsHost()
        {
            _isHostMode = true;
            Show();
            if (HostSection != null)   HostSection.SetActive(true);
            if (ClientSection != null) ClientSection.SetActive(false);
            if (JoinCodeDisplay != null) JoinCodeDisplay.text = "—";
            SetStatus("Press Start to host a game.");
            SetConfirmText("Start Game");
        }

        public void ShowAsClient()
        {
            _isHostMode = false;
            Show();
            if (HostSection != null)   HostSection.SetActive(false);
            if (ClientSection != null) ClientSection.SetActive(true);
            if (JoinCodeInput != null) JoinCodeInput.text = "";
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
            if (Root != null) Root.SetActive(false);
            NetworkSession.EnterHotseatMode();
        }

        // ── Host flow ──────────────────────────────────────────────────────────

        private async Task StartAsHostAsync()
        {
            SetStatus("Signing in to Unity Services…");
            await EnsureSignedInAsync();

            SetStatus("Allocating relay…");
            var allocation = await RelayService.Instance.CreateAllocationAsync(MaxClients);

            SetStatus("Requesting join code…");
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            if (JoinCodeDisplay != null) JoinCodeDisplay.text = joinCode;

            ConfigureTransportForHost(allocation);

            NetworkSession.EnterHostMode(joinCode);
            // Placeholder roster until Phase 5c gives the lobby a real player picker.
            // Host fills in the players; clients receive this list from the host.
            GameSession.SetDefault2PlayerHotseat();

            SetStatus("Starting host…");
            if (!NetworkManager.Singleton.StartHost())
            {
                throw new InvalidOperationException("NetworkManager.StartHost returned false.");
            }

            SetStatus("Loading game…");
            NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }

        // ── Client flow ────────────────────────────────────────────────────────

        private async Task StartAsClientAsync()
        {
            var joinCode = JoinCodeInput != null ? JoinCodeInput.text?.Trim() : null;
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
            {
                throw new InvalidOperationException("NetworkManager.StartClient returned false.");
            }
            // Host will load MainScene via NetworkManager.SceneManager; the client
            // follows automatically. Nothing more to do here.
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
            if (Root != null) Root.SetActive(true);
            SetConfirmInteractable(true);
        }

        private void SetStatus(string text)
        {
            if (StatusLabel != null) StatusLabel.text = text;
        }

        private void SetConfirmText(string text)
        {
            if (ConfirmButton == null) return;
            var label = ConfirmButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = text;
        }

        private void SetConfirmInteractable(bool interactable)
        {
            if (ConfirmButton != null) ConfirmButton.interactable = interactable;
        }
    }
}
