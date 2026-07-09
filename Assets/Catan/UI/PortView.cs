using UnityEngine;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Spawned at runtime by BoardRenderer. Requires a SpriteRenderer on the root
    // and an optional child SpriteRenderer named "ResourceIcon" for the resource icon.
    // BoardRenderer assigns the background sprite; the resource icon is resolved from
    // CatanResources at Initialize time.
    // ──────────────────────────────────────────────────────────────────────────

    public class PortView : MonoBehaviour
    {
        public SpriteRenderer BackgroundRenderer;
        public SpriteRenderer ResourceIconRenderer;

        private Port _port;

        public void Initialize(Port port, Sprite backgroundSprite)
        {
            _port = port;

            if (BackgroundRenderer != null)
                BackgroundRenderer.sprite = backgroundSprite;

            if (ResourceIconRenderer == null) return;

            if (port.SpecificResource.HasValue)
            {
                var catanResource = CatanResources.Get(port.SpecificResource.Value) as CatanResource;
                if (catanResource?.Icon != null)
                {
                    ResourceIconRenderer.sprite = catanResource.Icon;
                    ResourceIconRenderer.gameObject.SetActive(true);
                }
                else
                {
                    ResourceIconRenderer.gameObject.SetActive(false);
                }
            }
            else
            {
                ResourceIconRenderer.gameObject.SetActive(false);
            }
        }
    }
}
