using UnityEngine;
using UnityEngine.UIElements;
using Catan.Commands;
using GameCore.Resources;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class BankTradePanelView : MonoBehaviour
    {
        private static IResource[] AllResources =>
            new IResource[]
            {
                CatanResources.Wood, CatanResources.Brick, CatanResources.Sheep,
                CatanResources.Wheat, CatanResources.Ore
            };

        private VisualElement _panelRoot;

        private Button[] _giveButtons;
        private VisualElement[] _giveIcons;
        private Label[] _giveInfoLabels;

        private Button[] _receiveButtons;
        private VisualElement[] _receiveIcons;

        private Button _confirmButton;
        private Label  _feedbackLabel;

        private int _giveIndex    = -1;
        private int _receiveIndex = -1;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _panelRoot = root.Q<VisualElement>("BankTradePanelRoot");

            _giveButtons = new[]
            {
                root.Q<Button>("GiveWoodBtn"),
                root.Q<Button>("GiveBrickBtn"),
                root.Q<Button>("GiveSheepBtn"),
                root.Q<Button>("GiveWheatBtn"),
                root.Q<Button>("GiveOreBtn")
            };

            _giveIcons = new[]
            {
                root.Q<VisualElement>("GiveWoodIcon"),
                root.Q<VisualElement>("GiveBrickIcon"),
                root.Q<VisualElement>("GiveSheepIcon"),
                root.Q<VisualElement>("GiveWheatIcon"),
                root.Q<VisualElement>("GiveOreIcon")
            };

            _giveInfoLabels = new[]
            {
                root.Q<Label>("GiveWoodInfo"),
                root.Q<Label>("GiveBrickInfo"),
                root.Q<Label>("GiveSheepInfo"),
                root.Q<Label>("GiveWheatInfo"),
                root.Q<Label>("GiveOreInfo")
            };

            _receiveButtons = new[]
            {
                root.Q<Button>("ReceiveWoodBtn"),
                root.Q<Button>("ReceiveBrickBtn"),
                root.Q<Button>("ReceiveSheepBtn"),
                root.Q<Button>("ReceiveWheatBtn"),
                root.Q<Button>("ReceiveOreBtn")
            };

            _receiveIcons = new[]
            {
                root.Q<VisualElement>("ReceiveWoodIcon"),
                root.Q<VisualElement>("ReceiveBrickIcon"),
                root.Q<VisualElement>("ReceiveSheepIcon"),
                root.Q<VisualElement>("ReceiveWheatIcon"),
                root.Q<VisualElement>("ReceiveOreIcon")
            };

            _confirmButton = root.Q<Button>("BankTradeConfirmBtn");
            _feedbackLabel = root.Q<Label>("BankTradeFeedbackLabel");

            root.Q<Button>("BankTradeCancelBtn")?.RegisterCallback<ClickEvent>(_ => Close());
            _confirmButton?.RegisterCallback<ClickEvent>(_ => OnConfirm());

            for (int buttonIndex = 0; buttonIndex < _giveButtons.Length; buttonIndex++)
            {
                int capturedIndex = buttonIndex;
                _giveButtons[buttonIndex]?.RegisterCallback<ClickEvent>(_ => OnGiveClicked(capturedIndex));
            }

            for (int buttonIndex = 0; buttonIndex < _receiveButtons.Length; buttonIndex++)
            {
                int capturedIndex = buttonIndex;
                _receiveButtons[buttonIndex]?.RegisterCallback<ClickEvent>(_ => OnReceiveClicked(capturedIndex));
            }

            ApplyResourceCardSprites();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void Open()
        {
            _giveIndex    = -1;
            _receiveIndex = -1;
            if (_feedbackLabel != null) _feedbackLabel.text = "";
            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.Flex;
            RefreshUI();
        }

        public void Close()
        {
            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.None;
        }

        // ── Button callbacks ───────────────────────────────────────────────────

        private void OnGiveClicked(int index)
        {
            _giveIndex = (_giveIndex == index) ? -1 : index;
            if (_feedbackLabel != null) _feedbackLabel.text = "";
            RefreshUI();
        }

        private void OnReceiveClicked(int index)
        {
            _receiveIndex = (_receiveIndex == index) ? -1 : index;
            if (_feedbackLabel != null) _feedbackLabel.text = "";
            RefreshUI();
        }

        private void OnConfirm()
        {
            if (_giveIndex < 0 || _receiveIndex < 0 || _giveIndex == _receiveIndex) return;

            var giveResource    = AllResources[_giveIndex];
            var receiveResource = AllResources[_receiveIndex];

            var activePlayer  = GameManager.Instance?.ActivePlayer;
            int countBefore   = activePlayer?.Resources.Current.Get(giveResource) ?? 0;

            CommandDispatcher.Send(new BankTradeCommand { Give = giveResource, Receive = receiveResource });

            int countAfter = activePlayer?.Resources.Current.Get(giveResource) ?? 0;
            bool tradeSucceeded = countAfter < countBefore;

            if (_feedbackLabel != null)
                _feedbackLabel.text = tradeSucceeded ? "Trade complete!" : "Trade failed.";

            if (tradeSucceeded)
            {
                _giveIndex    = -1;
                _receiveIndex = -1;
                RefreshUI();
            }
        }

        // ── UI refresh ─────────────────────────────────────────────────────────

        private void ApplyResourceCardSprites()
        {
            var allResources = AllResources;
            for (int resourceIndex = 0; resourceIndex < allResources.Length; resourceIndex++)
            {
                var sprite = (allResources[resourceIndex] as CatanResource)?.Icon;
                if (sprite == null) continue;

                if (resourceIndex < _giveIcons.Length)
                    _giveIcons[resourceIndex]?.ApplySprite(sprite);

                if (resourceIndex < _receiveIcons.Length)
                    _receiveIcons[resourceIndex]?.ApplySprite(sprite);
            }
        }

        private void RefreshUI()
        {
            var gameManager  = GameManager.Instance;
            var activePlayer = gameManager?.ActivePlayer;
            var allResources = AllResources;

            for (int resourceIndex = 0; resourceIndex < allResources.Length; resourceIndex++)
            {
                var resource           = allResources[resourceIndex];
                int count              = activePlayer?.Resources.Current.Get(resource) ?? 0;
                int ratio              = gameManager?.GetBankTradeRatio(resource) ?? 4;
                bool canAffordToGive   = count >= ratio;
                bool isGiveSelected    = _giveIndex == resourceIndex;
                bool isReceiveSelected = _receiveIndex == resourceIndex;

                if (resourceIndex < _giveButtons.Length)
                {
                    _giveButtons[resourceIndex]?.SetEnabled(canAffordToGive);
                    UpdateSelectedClass(_giveButtons[resourceIndex], isGiveSelected);
                }

                if (resourceIndex < _giveInfoLabels.Length && _giveInfoLabels[resourceIndex] != null)
                    _giveInfoLabels[resourceIndex].text = $"{count} (÷{ratio})";

                if (resourceIndex < _receiveButtons.Length)
                    UpdateSelectedClass(_receiveButtons[resourceIndex], isReceiveSelected);
            }

            bool canConfirm = _giveIndex >= 0 && _receiveIndex >= 0 && _giveIndex != _receiveIndex;
            _confirmButton?.SetEnabled(canConfirm);
        }

        private static void UpdateSelectedClass(Button button, bool isSelected)
        {
            if (button == null) return;
            if (isSelected) button.AddToClassList("btn--selected");
            else            button.RemoveFromClassList("btn--selected");
        }
    }

    internal static class VisualElementSpriteExtensions
    {
        internal static void ApplySprite(this VisualElement element, UnityEngine.Sprite sprite)
        {
            if (element != null && sprite != null)
                element.style.backgroundImage = new StyleBackground(sprite);
        }
    }
}
