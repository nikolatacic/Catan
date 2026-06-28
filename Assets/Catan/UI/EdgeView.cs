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
                bool edgeEmpty = manager.Board != null && !manager.Board.Roads.ContainsKey(Edge);
                var player = manager.ActivePlayer;
                var roadColor = player != null ? player.Color : Color.white;
                SetHighlight(edgeEmpty, new Color(roadColor.r, roadColor.g, roadColor.b, 0.6f));
            }
            else
            {
                SetHighlight(false, Color.clear);
            }
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
