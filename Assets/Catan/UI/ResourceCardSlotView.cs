using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Catan.UI
{
    // ── Scene setup ────────────────────────────────────────────────────────────
    // Place 5 of these as children of a HorizontalLayoutGroup inside PlayerHandView.
    // Wire CardBackground (Image), ResourceIcon (Image, optional), CountLabel (TMP).
    // Assign each slot's CatanResource in the Inspector to fix its resource type.
    // ──────────────────────────────────────────────────────────────────────────

    public class ResourceCardSlotView : MonoBehaviour
    {
        [Header("Visuals")]
        public Image ResourceIcon;
        public TextMeshProUGUI CountLabel;

        [Header("Resource (assign in Inspector)")]
        public CatanResource Resource;

        private void Start()
        {
            ApplyResourceStyle();
            Refresh(0);
        }

        private void ApplyResourceStyle()
        {
            if (Resource == null) return;
            if (ResourceIcon != null)
            {
                var sprite = (Resource as CatanResource)?.Icon;
                ResourceIcon.sprite = sprite;
                ResourceIcon.enabled = sprite != null;
            }
        }

        public void Refresh(int count)
        {
            gameObject.SetActive(true);
            if (CountLabel != null)
                CountLabel.text = count.ToString();
        }
    }
}
