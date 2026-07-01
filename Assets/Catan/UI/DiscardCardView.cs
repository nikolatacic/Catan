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

        public IResource Resource { get; private set; }

        private Action<DiscardCardView> _onClick;

        public void Initialize(IResource resource, Action<DiscardCardView> onClick)
        {
            Resource = resource;
            _onClick = onClick;

            if (ResourceIcon != null)
            {
                var sprite = (resource as CatanResource)?.Icon;
                ResourceIcon.sprite = sprite;
                ResourceIcon.enabled = sprite != null;
            }

            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            GetComponent<Button>()?.onClick.RemoveListener(OnClicked);
        }

        public void OnClicked() => _onClick?.Invoke(this);
    }
}
