using UnityEngine;
using UnityEngine.UIElements;

namespace Catan.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class YearOfPlentyPanelView : MonoBehaviour
    {
        private static readonly CatanResourceType[] ResourceOrder =
        {
            CatanResourceType.Wood, CatanResourceType.Brick, CatanResourceType.Sheep,
            CatanResourceType.Wheat, CatanResourceType.Ore
        };

        private VisualElement   _panelRoot;
        private Button[]        _resourceButtons;
        private VisualElement[] _resourceIcons;
        private Label           _instructionLabel;
        private Button          _confirmButton;

        private YearOfPlentyCard  _card;
        private CatanGameContext  _context;
        private int               _firstSelection  = -1;
        private int               _secondSelection = -1;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _panelRoot = root.Q<VisualElement>("YearOfPlentyPanelRoot");
            _instructionLabel = root.Q<Label>("YearOfPlentyInstructionLabel");

            _resourceButtons = new[]
            {
                root.Q<Button>("YopWoodBtn"),
                root.Q<Button>("YopBrickBtn"),
                root.Q<Button>("YopSheepBtn"),
                root.Q<Button>("YopWheatBtn"),
                root.Q<Button>("YopOreBtn")
            };

            _resourceIcons = new[]
            {
                root.Q<VisualElement>("YopWoodIcon"),
                root.Q<VisualElement>("YopBrickIcon"),
                root.Q<VisualElement>("YopSheepIcon"),
                root.Q<VisualElement>("YopWheatIcon"),
                root.Q<VisualElement>("YopOreIcon")
            };

            _confirmButton = root.Q<Button>("YopConfirmBtn");
            _confirmButton?.RegisterCallback<ClickEvent>(_ => OnConfirm());
            root.Q<Button>("YopCancelBtn")?.RegisterCallback<ClickEvent>(_ => OnCancel());

            for (int buttonIndex = 0; buttonIndex < _resourceButtons.Length; buttonIndex++)
            {
                int capturedIndex = buttonIndex;
                _resourceButtons[buttonIndex]?.RegisterCallback<ClickEvent>(_ => OnResourceClicked(capturedIndex));
            }

            ApplyResourceIcons();
            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.None;
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void Open(YearOfPlentyCard card, CatanGameContext context)
        {
            _card            = card;
            _context         = context;
            _firstSelection  = -1;
            _secondSelection = -1;
            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.Flex;
            RefreshUI();
        }

        // ── Callbacks ──────────────────────────────────────────────────────────

        private void OnResourceClicked(int index)
        {
            if (_firstSelection == index)
            {
                // Deselect first; promote second to first if present
                _firstSelection  = _secondSelection;
                _secondSelection = -1;
            }
            else if (_secondSelection == index)
            {
                _secondSelection = -1;
            }
            else if (_firstSelection < 0)
            {
                _firstSelection = index;
            }
            else if (_secondSelection < 0)
            {
                _secondSelection = index;
            }
            else
            {
                // Both slots full — replace second with new pick
                _secondSelection = index;
            }

            RefreshUI();
        }

        private void OnConfirm()
        {
            if (_firstSelection < 0 || _card == null) return;

            _card.FirstChosenResource  = ResourceOrder[_firstSelection];
            _card.SecondChosenResource = _secondSelection >= 0
                ? ResourceOrder[_secondSelection]
                : ResourceOrder[_firstSelection];

            var cardToComplete    = _card;
            var contextToComplete = _context;
            _card            = null;
            _context         = null;
            _firstSelection  = -1;
            _secondSelection = -1;

            if (_panelRoot != null) _panelRoot.style.display = DisplayStyle.None;
            GameManager.Instance?.CompleteDevCardExecution(cardToComplete, contextToComplete);
        }

        private void OnCancel()
        {
            _card            = null;
            _context         = null;
            _firstSelection  = -1;
            _secondSelection = -1;
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
            _confirmButton?.SetEnabled(_firstSelection >= 0);

            if (_instructionLabel != null)
            {
                if (_firstSelection < 0)
                    _instructionLabel.text = "Choose your first resource.";
                else if (_secondSelection < 0)
                    _instructionLabel.text = "Choose your second resource (or confirm for two of the same).";
                else
                    _instructionLabel.text = "Confirm to receive both resources.";
            }

            for (int buttonIndex = 0; buttonIndex < _resourceButtons.Length; buttonIndex++)
            {
                var button = _resourceButtons[buttonIndex];
                if (button == null) continue;

                bool isFirstPick  = _firstSelection  == buttonIndex;
                bool isSecondPick = _secondSelection == buttonIndex;

                if (isFirstPick || isSecondPick) button.AddToClassList("btn--selected");
                else                             button.RemoveFromClassList("btn--selected");
            }
        }
    }
}
