using UnityEngine;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Spawned at runtime by BoardRenderer. Uses a BoxCollider2D for clicks.
    // ──────────────────────────────────────────────────────────────────────────

    public class EdgeView : MonoBehaviour
    {
        [Header("Renderers")]
        public SpriteRenderer RoadRenderer;

        public GameCore.Board.HexEdge Edge { get; private set; }

        public void Initialize(GameCore.Board.HexEdge edge)
        {
            Edge = edge;
            Refresh();
        }

        private void OnMouseDown()
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.CurrentPlacementMode == PlacementMode.Road)
                GameManager.Instance.TryPlaceRoad(Edge);

            Refresh();
        }

        public void Refresh()
        {
            if (RoadRenderer == null) return;

            var board = GameManager.Instance?.Board;
            if (board == null || Edge == null)
            {
                RoadRenderer.enabled = false;
                return;
            }

            if (!board.Roads.TryGetValue(Edge, out var road))
            {
                RoadRenderer.enabled = false;
                return;
            }

            RoadRenderer.enabled = true;
            if (road.Owner is CatanPlayer catanPlayer)
                RoadRenderer.color = catanPlayer.Color;
        }
    }
}
