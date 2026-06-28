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
            gameObject.SetActive(false);
        }

        private void ApplyResourceStyle()
        {
            if (Resource == null) return;
        }

        public void Refresh(int count)
        {
            gameObject.SetActive(count > 0);
            if (CountLabel != null)
                CountLabel.text = count.ToString();
        }
    }
}
