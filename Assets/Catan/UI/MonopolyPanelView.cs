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
    //                            and/or an Image child for the resource icon (auto-assigned).
    //   ConfirmButton          — wire OnClick → OnConfirm()
    //   CancelButton           — wire OnClick → OnCancel()
    //
    // Wire GameManager.MonopolyPanel → this component.
    // ──────────────────────────────────────────────────────────────────────────

    public class MonopolyPanelView : MonoBehaviour
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

        private MonopolyCard _card;
        private CatanGameContext _context;
        private int _selectedIndex = -1;

        private void Awake()
        {
            gameObject.SetActive(false);
            for (int buttonIndex = 0; buttonIndex < ResourceButtons.Length; buttonIndex++)
            {
                int capturedIndex = buttonIndex;
                ResourceButtons[buttonIndex]?.onClick.AddListener(() => OnResourceClicked(capturedIndex));
            }
        }

        public void Open(MonopolyCard card, CatanGameContext context)
        {
            _card = card;
            _context = context;
            _selectedIndex = -1;
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
            _selectedIndex = (_selectedIndex == index) ? -1 : index;
            RefreshUI();
        }

        public void OnConfirm()
        {
            if (_selectedIndex < 0 || _card == null) return;

            _card.ChosenResourceType = ResourceOrder[_selectedIndex];

            var card = _card;
            var context = _context;
            _card = null;
            _context = null;
            _selectedIndex = -1;

            gameObject.SetActive(false);
            GameManager.Instance?.CompleteDevCardExecution(card, context);
        }

        public void OnCancel()
        {
            _card = null;
            _context = null;
            _selectedIndex = -1;
            gameObject.SetActive(false);
        }

        private void RefreshUI()
        {
            if (ConfirmButton != null)
                ConfirmButton.interactable = _selectedIndex >= 0;

            for (int buttonIndex = 0; buttonIndex < ResourceButtons.Length; buttonIndex++)
            {
                if (ResourceButtons[buttonIndex] == null) continue;
                var image = ResourceButtons[buttonIndex].GetComponent<Image>();
                if (image != null)
                    image.color = (_selectedIndex == buttonIndex) ? Color.yellow : Color.white;
            }
        }
    }
}
