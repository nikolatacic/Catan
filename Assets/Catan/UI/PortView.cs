using UnityEngine;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Spawned at runtime by BoardRenderer. Requires a SpriteRenderer on the root
    // for the port icon (the sprite already contains the resource image and ratio
    // text). Two dock connector lines are created at Initialize time and connect
    // the port icon back to the two access vertices on the board edge.
    // ──────────────────────────────────────────────────────────────────────────

    public class PortView : MonoBehaviour
    {
        public SpriteRenderer PortIconRenderer;

        [Tooltip("Width of the dock connector lines in world units")]
        public float DockLineWidth = 0.08f;

        [Tooltip("Brown wood color used for the dock connector lines")]
        public Color DockColor = new Color(0.55f, 0.35f, 0.15f);

        public void Initialize(Port port, Sprite portSprite, Vector3 vertexPosA, Vector3 vertexPosB)
        {
            if (PortIconRenderer != null)
                PortIconRenderer.sprite = portSprite;

            SpawnDockConnector(transform.position, vertexPosA);
            SpawnDockConnector(transform.position, vertexPosB);
        }

        private void SpawnDockConnector(Vector3 fromPos, Vector3 toPos)
        {
            var dockGo = new GameObject("DockConnector");
            dockGo.transform.SetParent(transform, false);

            var lineRenderer = dockGo.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, fromPos);
            lineRenderer.SetPosition(1, toPos);
            lineRenderer.startWidth = DockLineWidth;
            lineRenderer.endWidth = DockLineWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = DockColor;
            lineRenderer.endColor = DockColor;
            lineRenderer.sortingOrder = -1;
        }
    }
}
