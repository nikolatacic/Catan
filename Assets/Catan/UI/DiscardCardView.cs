using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameCore.Resources;

namespace Catan.UI
{
    // ── Prefab setup ───────────────────────────────────────────────────────────
    // Add to a UI GameObject that also has a Button component.
    // Children needed:
    //   CardBackground  — Image (coloured backing)
    //   ResourceIcon    — Image (resource sprite, optional)
    //   ResourceLabel   — TextMeshProUGUI (resource name)
    // Wire the Button's OnClick → this.OnClicked() in the Inspector,
    // OR let DiscardPanelView add the listener via Initialize().
    // ──────────────────────────────────────────────────────────────────────────

    [RequireComponent(typeof(Button))]
    public class DiscardCardView : MonoBehaviour
    {
        [Header("Visuals")]
        public Image CardBackground;
        public Image ResourceIcon;
        public TextMeshProUGUI ResourceLabel;

        public IResource Resource { get; private set; }

        private Action<DiscardCardView> _onClick;

        public void Initialize(IResource resource, Action<DiscardCardView> onClick)
        {
            Resource = resource;
            _onClick = onClick;

            if (ResourceLabel != null)
                ResourceLabel.text = resource.DisplayName;

            if (resource is CatanResource catanResource)
            {
                if (CardBackground != null)
                    CardBackground.color = ResourceColor(catanResource.Type);

                if (ResourceIcon != null)
                {
                    ResourceIcon.sprite = catanResource.Icon;
                    ResourceIcon.enabled = catanResource.Icon != null;
                }
            }

            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            GetComponent<Button>()?.onClick.RemoveListener(OnClicked);
        }

        public void OnClicked() => _onClick?.Invoke(this);

        private static Color ResourceColor(CatanResourceType type) => type switch
        {
            CatanResourceType.Wood  => new Color(0.40f, 0.25f, 0.10f),
            CatanResourceType.Brick => new Color(0.80f, 0.30f, 0.10f),
            CatanResourceType.Sheep => new Color(0.55f, 0.85f, 0.35f),
            CatanResourceType.Wheat => new Color(0.95f, 0.85f, 0.20f),
            CatanResourceType.Ore   => new Color(0.50f, 0.50f, 0.60f),
            _                       => Color.white
        };
    }
}
