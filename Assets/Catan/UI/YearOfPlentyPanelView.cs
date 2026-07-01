using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Catan.UI
{
    // ── Scene setup ────────────────────────────────────────────────────────────
    // Full-screen overlay panel — add to Canvas as a sibling of other modals.
    // Starts active so Awake() fires on scene load, then deactivates itself.
    //
    // Assign in Inspector:
    //   ResourceButtons[0..4]  — one Button per resource (Wood, Brick, Sheep, Wheat, Ore)
    //                            Each button may have a TextMeshProUGUI child (auto-labelled)
    //                            and/or an Image child named "Icon" (auto-assigned sprite).
    //   ConfirmButton / CancelButton — assigned; onClick wired automatically in Awake().
    //   InstructionLabel       — updated at runtime
    //
    // Wire GameManager.YearOfPlentyPanel → this component.
    //
    // Picking rules:
    //   • Click once to select the first resource.
    //   • Click again (same or different) to select the second resource.
    //   • Click a selected resource a third time to deselect it.
    //   • Confirm is enabled as soon as at least one resource is selected
    //     (a single choice means both slots go to the same resource, which is
    //     legal in Catan).
    // ──────────────────────────────────────────────────────────────────────────

    public class YearOfPlentyPanelView : MonoBehaviour
    {
        private static readonly CatanResourceType[] ResourceOrder =
        {
            CatanResourceType.Wood, CatanResourceType.Brick, CatanResourceType.Sheep,
            CatanResourceType.Wheat, CatanResourceType.Ore
        };

        [Header("Resource buttons (Wood, Brick, Sheep, Wheat, Ore)")]
        public Button[] ResourceButtons;

        [Header("Controls")]
        public Button ConfirmButton;
        public Button CancelButton;
        public TextMeshProUGUI InstructionLabel;

        private YearOfPlentyCard _card;
        private CatanGameContext _context;
        private int _firstSelection  = -1;
        private int _secondSelection = -1;
        private Color[] _buttonDefaultColors;

        private void Awake()
        {
            gameObject.SetActive(false);
            _buttonDefaultColors = new Color[ResourceButtons.Length];
            for (int buttonIndex = 0; buttonIndex < ResourceButtons.Length; buttonIndex++)
            {
                var resourceButton = ResourceButtons[buttonIndex];
                if (resourceButton == null) continue;

                var buttonImage = resourceButton.GetComponent<Image>();
                if (buttonImage != null)
                    _buttonDefaultColors[buttonIndex] = buttonImage.color;

                int capturedIndex = buttonIndex;
                resourceButton.onClick.AddListener(() => OnResourceClicked(capturedIndex));
            }
            ConfirmButton?.onClick.AddListener(OnConfirm);
            CancelButton?.onClick.AddListener(OnCancel);
        }

        public void Open(YearOfPlentyCard card, CatanGameContext context)
        {
            _card = card;
            _context = context;
            _firstSelection  = -1;
            _secondSelection = -1;
            gameObject.SetActive(true);
            SetButtonLabels();
            RefreshUI();
        }

        private void SetButtonLabels()
        {
            for (int buttonIndex = 0; buttonIndex < ResourceButtons.Length; buttonIndex++)
            {
                if (buttonIndex >= ResourceOrder.Length) break;
                var button = ResourceButtons[buttonIndex];
                if (button == null) continue;

                var resource = CatanResources.Get(ResourceOrder[buttonIndex]) as CatanResource;

                var label = button.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null && resource != null)
                    label.text = resource.DisplayName;

                var icon = button.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null && resource != null)
                {
                    icon.sprite = resource.Icon;
                    icon.enabled = resource.Icon != null;
                }
            }
        }

        private void OnResourceClicked(int index)
        {
            if (_firstSelection == index)
            {
                // Deselect first; promote second to first if present.
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
                // Both slots full — replace second with the new pick.
                _secondSelection = index;
            }

            RefreshUI();
        }

        public void OnConfirm()
        {
            if (_firstSelection < 0 || _card == null) return;

            _card.FirstChosenResource  = ResourceOrder[_firstSelection];
            _card.SecondChosenResource = _secondSelection >= 0
                ? ResourceOrder[_secondSelection]
                : ResourceOrder[_firstSelection];

            var card    = _card;
            var context = _context;
            _card    = null;
            _context = null;
            _firstSelection  = -1;
            _secondSelection = -1;

            gameObject.SetActive(false);
            GameManager.Instance?.CompleteDevCardExecution(card, context);
        }

        public void OnCancel()
        {
            _card    = null;
            _context = null;
            _firstSelection  = -1;
            _secondSelection = -1;
            gameObject.SetActive(false);
        }

        private void RefreshUI()
        {
            if (ConfirmButton != null)
                ConfirmButton.interactable = _firstSelection >= 0;

            if (InstructionLabel != null)
            {
                if (_firstSelection < 0)
                    InstructionLabel.text = "Choose first resource";
                else if (_secondSelection < 0)
                    InstructionLabel.text = "Choose second resource (or confirm for two of the same)";
                else
                    InstructionLabel.text = "Confirm to receive both resources";
            }

            for (int buttonIndex = 0; buttonIndex < ResourceButtons.Length; buttonIndex++)
            {
                if (ResourceButtons[buttonIndex] == null) continue;
                var buttonImage = ResourceButtons[buttonIndex].GetComponent<Image>();
                if (buttonImage == null) continue;

                bool isFirstPick  = _firstSelection  == buttonIndex;
                bool isSecondPick = _secondSelection == buttonIndex;
                buttonImage.color = (isFirstPick || isSecondPick)
                    ? Color.yellow
                    : (_buttonDefaultColors != null && buttonIndex < _buttonDefaultColors.Length
                        ? _buttonDefaultColors[buttonIndex]
                        : Color.white);
            }
        }
    }
}
