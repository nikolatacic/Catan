using UnityEngine;
using Catan.Commands;
using GameCore.Events;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Spawned at runtime by BoardRenderer. Uses a BoxCollider2D for clicks.
    //
    // Highlight setup (two options, either works):
    //   A) Add a child GameObject named exactly "Highlight" with a SpriteRenderer.
    //      It will be found automatically at startup.
    //   B) Assign any SpriteRenderer to the HighlightRenderer field in the prefab.
    // ──────────────────────────────────────────────────────────────────────────

    public class EdgeView : MonoBehaviour
    {
        [Header("Renderers")]
        public SpriteRenderer RoadRenderer;
        public SpriteRenderer HighlightRenderer;

        public GameCore.Board.HexEdge Edge { get; private set; }

        private void Awake()
        {
            if (HighlightRenderer == null)
            {
                var highlightTransform = transform.Find("Highlight");
                if (highlightTransform != null)
                    HighlightRenderer = highlightTransform.GetComponent<SpriteRenderer>();
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlacementModeChangedEvent>(OnPlacementModeChanged);
            EventBus.Subscribe<GameCore.Build.BuildSucceededEvent>(OnBuildSucceeded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlacementModeChangedEvent>(OnPlacementModeChanged);
            EventBus.Unsubscribe<GameCore.Build.BuildSucceededEvent>(OnBuildSucceeded);
        }

        public void Initialize(GameCore.Board.HexEdge edge)
        {
            Edge = edge;
            SetHighlight(false, Color.clear);
            Refresh();
        }

        private void OnMouseDown()
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.CurrentPlacementMode == PlacementMode.Road)
                CommandDispatcher.Send(new PlaceRoadCommand { Edge = Edge });

            Refresh();
        }

        private void OnPlacementModeChanged(PlacementModeChangedEvent gameEvent)
        {
            UpdateHighlight(gameEvent.Mode);
        }

        private void OnBuildSucceeded(GameCore.Build.BuildSucceededEvent gameEvent) => Refresh();

        private void UpdateHighlight(PlacementMode mode)
        {
            var manager = GameManager.Instance;
            if (manager == null || Edge == null)
            {
                SetHighlight(false, Color.clear);
                return;
            }

            if (mode == PlacementMode.Road)
            {
                var player = manager.ActivePlayer;
                bool canPlaceRoad = manager.Board != null
                    && !manager.Board.Roads.ContainsKey(Edge)
                    && HasConnectionForRoad(manager.Board, player);
                var roadColor = player != null ? player.Color : Color.white;
                SetHighlight(canPlaceRoad, new Color(roadColor.r, roadColor.g, roadColor.b, 0.6f));
            }
            else
            {
                SetHighlight(false, Color.clear);
            }
        }

        // Mirrors CatanBuildRule.HasRoadConnectionAtEdge — player's own settlement at
        // an endpoint is a valid connection; opponent's settlement blocks that endpoint;
        // empty endpoint is valid if the player has any adjacent road through it.
        private bool HasConnectionForRoad(CatanBoard board, CatanPlayer player)
        {
            if (player == null || Edge.AdjacentVertices == null) return false;
            foreach (var endpointVertex in Edge.AdjacentVertices)
            {
                if (board.Settlements.TryGetValue(endpointVertex, out var settlement))
                {
                    if (settlement.Owner == player) return true;
                    continue; // opponent's settlement blocks this endpoint
                }
                if (endpointVertex.AdjacentEdges == null) continue;
                foreach (var neighbourEdge in endpointVertex.AdjacentEdges)
                {
                    if (neighbourEdge == Edge) continue;
                    if (board.Roads.TryGetValue(neighbourEdge, out var neighbourRoad)
                        && neighbourRoad.Owner == player)
                        return true;
                }
            }
            return false;
        }

        private void SetHighlight(bool visible, Color color)
        {
            if (HighlightRenderer == null) return;
            HighlightRenderer.enabled = visible;
            if (visible) HighlightRenderer.color = color;
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
