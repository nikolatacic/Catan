using UnityEngine;
using UnityEngine.UIElements;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MonopolyPanelView : MonoBehaviour
    {
        private static readonly CatanResourceType[] ResourceOrder =
        {
            CatanResourceType.Wood, CatanResourceType.Brick, CatanResourceType.Sheep,
            CatanResourceType.Wheat, CatanResourceType.Ore
        };

        private VisualElement _panelRoot;
        private Button[]      _resourceButtons;
        private VisualElement[] _resourceIcons;
        private Button        _confirmButton;

        private MonopolyCard       _card;
        private CatanGameContext   _context;
        private int                _selectedIndex = -1;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _panelRoot = root.Q<VisualElement>("MonopolyPanelRoot");

            _resourceButtons = new[]
            {
                root.Q<Button>("MonopolyWoodBtn"),
                root.Q<Button>("MonopolyBrickBtn"),
                root.Q<Button>("MonopolySheepBtn"),
                root.Q<Button>("MonopolyWheatBtn"),
                root.Q<Button>("MonopolyOreBtn")
            };

            _resourceIcons = new[]
            {
                root.Q<VisualElement>("MonopolyWoodIcon"),
                root.Q<VisualElement>("MonopolyBrickIcon"),
                root.Q<VisualElement>("MonopolySheepIcon"),
                root.Q<VisualElement>("MonopolyWheatIcon"),
                root.Q<VisualElement>("MonopolyOreIcon")
            };

            _confirmButton = root.Q<Button>("MonopolyConfirmBtn");
            _confirmButton?.RegisterCallback<ClickEvent>(_ => OnConfirm());
            root.Q<Button>("MonopolyCancelBtn")?.RegisterCallback<ClickEvent>(_ => OnCancel());

            for (int buttonIndex = 0; buttonIndex < _resourceButtons.Length; buttonIndex++)
            {
                int capturedIndex = buttonIndex;
                _resourceButtons[buttonIndex]?.RegisterCallback<ClickEvent>(_ => OnResourceClicked(capturedIndex));
            }

            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.None;
        }

        private void Start()
        {
            _panelRoot?.StretchTemplateContainerToFill();
            ApplyResourceIcons();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void Open(MonopolyCard card, CatanGameContext context)
        {
            _card          = card;
            _context       = context;
            _selectedIndex = -1;
            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.Flex;
            RefreshUI();
        }

        // ── Callbacks ──────────────────────────────────────────────────────────

        private void OnResourceClicked(int index)
        {
            _selectedIndex = (_selectedIndex == index) ? -1 : index;
            RefreshUI();
        }

        private void OnConfirm()
        {
            if (_selectedIndex < 0 || _card == null) return;

            _card.ChosenResourceType = ResourceOrder[_selectedIndex];

            var cardToComplete    = _card;
            var contextToComplete = _context;
            _card          = null;
            _context       = null;
            _selectedIndex = -1;

            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.None;
            GameManager.Instance?.CompleteDevCardExecution(cardToComplete, contextToComplete);
        }

        private void OnCancel()
        {
            _card          = null;
            _context       = null;
            _selectedIndex = -1;
            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.None;
        }

        // ── UI ─────────────────────────────────────────────────────────────────

        private void ApplyResourceIcons()
        {
            for (int resourceIndex = 0; resourceIndex < ResourceOrder.Length; resourceIndex++)
            {
                var catanResource = CatanResources.Get(ResourceOrder[resourceIndex]) as CatanResource;
                if (catanResource?.Icon == null) continue;
                if (resourceIndex < _resourceIcons.Length)
                    _resourceIcons[resourceIndex]?.ApplySprite(catanResource.Icon);
            }
        }

        private void RefreshUI()
        {
            _confirmButton?.SetEnabled(_selectedIndex >= 0);

            for (int buttonIndex = 0; buttonIndex < _resourceButtons.Length; buttonIndex++)
            {
                var button = _resourceButtons[buttonIndex];
                if (button == null) continue;

                bool isSelected = _selectedIndex == buttonIndex;
                if (isSelected) button.AddToClassList("btn--selected");
                else            button.RemoveFromClassList("btn--selected");
            }
        }
    }
}
