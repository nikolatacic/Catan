using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Catan.Commands;
using GameCore.Resources;

namespace Catan.UI
{
    // ── Scene setup ────────────────────────────────────────────────────────────
    // Add a panel to Canvas (starts inactive). Assign:
    //   GiveButtons[0..4]    — 5 Buttons in order: Wood, Brick, Sheep, Wheat, Ore
    //   ReceiveButtons[0..4] — same order
    //   ConfirmButton, CancelButton, FeedbackLabel
    //
    // Each resource button needs one TextMeshProUGUI child for the label.
    // Wire ActionButtonsView.BankTradePanel → this GameObject.
    // Wire CancelButton.OnClick → BankTradePanelView.Close()
    // Wire ConfirmButton.OnClick → BankTradePanelView.OnConfirm()
    // GiveButtons and ReceiveButtons have listeners added in code — no Inspector
    // OnClick wiring needed for those.
    // ──────────────────────────────────────────────────────────────────────────

    public class BankTradePanelView : MonoBehaviour
    {
        [Header("Give row (Wood / Brick / Sheep / Wheat / Ore)")]
        public Button[] GiveButtons;

        [Header("Receive row (same order)")]
        public Button[] ReceiveButtons;

        [Header("Controls")]
        public Button ConfirmButton;
        public Button CancelButton;
        public TextMeshProUGUI FeedbackLabel;

        private static IResource[] AllResources =>
            new IResource[]
            {
                CatanResources.Wood, CatanResources.Brick, CatanResources.Sheep,
                CatanResources.Wheat, CatanResources.Ore
            };

        private int _giveIndex    = -1;
        private int _receiveIndex = -1;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            gameObject.SetActive(false);
            WireButtonListeners();
        }

        private void WireButtonListeners()
        {
            for (int buttonIndex = 0; buttonIndex < GiveButtons.Length; buttonIndex++)
            {
                int capturedIndex = buttonIndex;
                GiveButtons[buttonIndex]?.onClick.AddListener(() => OnGiveClicked(capturedIndex));
            }

            for (int buttonIndex = 0; buttonIndex < ReceiveButtons.Length; buttonIndex++)
            {
                int capturedIndex = buttonIndex;
                ReceiveButtons[buttonIndex]?.onClick.AddListener(() => OnReceiveClicked(capturedIndex));
            }
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void Open()
        {
            _giveIndex    = -1;
            _receiveIndex = -1;
            if (FeedbackLabel != null) FeedbackLabel.text = "";
            gameObject.SetActive(true);
            RefreshUI();
        }

        public void Close() => gameObject.SetActive(false);

        // ── Button callbacks ───────────────────────────────────────────────────

        private void OnGiveClicked(int index)
        {
            _giveIndex = (_giveIndex == index) ? -1 : index;
            if (FeedbackLabel != null) FeedbackLabel.text = "";
            RefreshUI();
        }

        private void OnReceiveClicked(int index)
        {
            _receiveIndex = (_receiveIndex == index) ? -1 : index;
            if (FeedbackLabel != null) FeedbackLabel.text = "";
            RefreshUI();
        }

        public void OnConfirm()
        {
            if (_giveIndex < 0 || _receiveIndex < 0 || _giveIndex == _receiveIndex) return;

            var giveResource    = AllResources[_giveIndex];
            var receiveResource = AllResources[_receiveIndex];

            // Snapshot resource counts so we can detect success after the
            // command runs (works for local hotseat and forward-compatible
            // with multiplayer where the trade is async).
            var player = GameManager.Instance?.ActivePlayer;
            int giveBefore = player?.Resources.Current.Get(giveResource) ?? 0;

            CommandDispatcher.Send(new BankTradeCommand { Give = giveResource, Receive = receiveResource });

            int giveAfter = player?.Resources.Current.Get(giveResource) ?? 0;
            bool success = giveAfter < giveBefore;

            if (FeedbackLabel != null)
                FeedbackLabel.text = success ? "Trade complete!" : "Trade failed.";

            if (success)
            {
                _giveIndex    = -1;
                _receiveIndex = -1;
                RefreshUI();
            }
        }

        // ── UI refresh ─────────────────────────────────────────────────────────

        private void RefreshUI()
        {
            var manager = GameManager.Instance;
            var player  = manager?.ActivePlayer;

            for (int resourceIndex = 0; resourceIndex < AllResources.Length; resourceIndex++)
            {
                var resource  = AllResources[resourceIndex];
                int count     = player?.Resources.Current.Get(resource) ?? 0;
                int ratio     = manager?.GetBankTradeRatio(resource) ?? 4;
                bool canGive  = count >= ratio;

                UpdateGiveButton(resourceIndex, resource, count, ratio, canGive);
                UpdateReceiveButton(resourceIndex, resource);
            }

            bool canConfirm = _giveIndex >= 0 && _receiveIndex >= 0
                              && _giveIndex != _receiveIndex;
            if (ConfirmButton != null) ConfirmButton.interactable = canConfirm;
        }

        private void UpdateGiveButton(int index, IResource resource, int count, int ratio, bool canGive)
        {
            if (index >= GiveButtons.Length || GiveButtons[index] == null) return;

            var button = GiveButtons[index];
            button.interactable = canGive;

            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = $"{resource.DisplayName}\n{count} (÷{ratio})";

            var image = button.GetComponent<Image>();
            var resourceColor = (resource as CatanResource)?.Color ?? Color.white;
            if (image != null)
                image.color = _giveIndex == index
                    ? Color.yellow
                    : canGive ? resourceColor : new Color(0.25f, 0.25f, 0.25f);
        }

        private void UpdateReceiveButton(int index, IResource resource)
        {
            if (index >= ReceiveButtons.Length || ReceiveButtons[index] == null) return;

            var button = ReceiveButtons[index];
            button.interactable = _receiveIndex != index || true; // always clickable

            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = resource.DisplayName;

            var image = button.GetComponent<Image>();
            if (image != null)
                image.color = _receiveIndex == index
                    ? Color.yellow
                    : (resource as CatanResource)?.Color ?? Color.white;
        }
    }
}
