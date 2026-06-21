using System.Collections.Generic;
using UnityEngine;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Attach to a child GameObject of GameManager ("BoardRenderer").
    // Assign HexTilePrefab, VertexPrefab, EdgePrefab.
    // Assign TileSprites array indexed by CatanResourceType enum value.
    // GameManager calls RenderBoard() after the board is generated.
    // ──────────────────────────────────────────────────────────────────────────

    public class BoardRenderer : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject HexTilePrefab;
        public GameObject VertexPrefab;
        public GameObject EdgePrefab;

        [Header("Tile Sprites (indexed by CatanResourceType, last index = Desert)")]
        public Sprite[] TileSprites;
        public Sprite DesertSprite;

        [Header("Layout")]
        public float HexSize = 1.0f;

        private readonly List<HexTileView> _tileViews = new();
        private readonly List<VertexView> _vertexViews = new();
        private readonly List<EdgeView> _edgeViews = new();

        public void RenderBoard()
        {
            var board = GameManager.Instance?.Board;
            if (board == null) return;

            ClearBoard();

            var spawnedVertices = new HashSet<GameCore.Board.HexVertex>();
            var spawnedEdges = new HashSet<GameCore.Board.HexEdge>();

            foreach (var tile in board.Grid.Tiles.Values)
            {
                SpawnTile(tile);

                var tileVertices = board.Grid.GetVertices(tile.Coord);
                for (int cornerIndex = 0; cornerIndex < tileVertices.Count; cornerIndex++)
                {
                    var vertex = tileVertices[cornerIndex];
                    if (!spawnedVertices.Add(vertex)) continue;
                    SpawnVertex(vertex, tile.Coord, cornerIndex);
                }

                var tileEdges = board.Grid.GetEdges(tile.Coord);
                for (int edgeIndex = 0; edgeIndex < tileEdges.Count; edgeIndex++)
                {
                    var edge = tileEdges[edgeIndex];
                    if (!spawnedEdges.Add(edge)) continue;
                    SpawnEdge(edge, tile.Coord, edgeIndex);
                }
            }
        }

        private void SpawnTile(CatanHexTile tile)
        {
            if (HexTilePrefab == null) return;

            var worldPos = HexToWorld(tile.Coord);
            var go = Instantiate(HexTilePrefab, worldPos, Quaternion.identity, transform);
            go.name = $"Tile_{tile.Coord}";

            var tileView = go.GetComponent<HexTileView>();
            if (tileView == null) return;

            tileView.Initialize(tile, GetTileSprite(tile.ResourceType));
            _tileViews.Add(tileView);
        }

        private void SpawnVertex(GameCore.Board.HexVertex vertex,
            GameCore.Board.HexCoord tileCoord, int cornerIndex)
        {
            if (VertexPrefab == null) return;

            var worldPos = CornerWorldPosition(tileCoord, cornerIndex);
            var go = Instantiate(VertexPrefab, worldPos, Quaternion.identity, transform);
            go.name = $"Vertex_{vertex.GetHashCode()}";

            var vertexView = go.GetComponent<VertexView>();
            if (vertexView == null) return;

            vertexView.Initialize(vertex);
            _vertexViews.Add(vertexView);
        }

        private void SpawnEdge(GameCore.Board.HexEdge edge,
            GameCore.Board.HexCoord tileCoord, int edgeIndex)
        {
            if (EdgePrefab == null) return;

            // Edge at index i connects corners (i-1)%6 and i.
            var posA = CornerWorldPosition(tileCoord, (edgeIndex + 5) % 6);
            var posB = CornerWorldPosition(tileCoord, edgeIndex);
            var worldPos = (posA + posB) * 0.5f;

            // Long axis of edge i is at -60*(i+1) degrees from horizontal.
            var rotation = Quaternion.Euler(0f, 0f, -60f * (edgeIndex + 1));
            var go = Instantiate(EdgePrefab, worldPos, rotation, transform);
            go.name = $"Edge_{edge.GetHashCode()}";

            var edgeView = go.GetComponent<EdgeView>();
            if (edgeView == null) return;

            edgeView.Initialize(edge);
            _edgeViews.Add(edgeView);
        }

        public void RefreshAll()
        {
            foreach (var vertexView in _vertexViews) vertexView.Refresh();
            foreach (var edgeView in _edgeViews) edgeView.Refresh();
        }

        private void ClearBoard()
        {
            foreach (var view in _tileViews) if (view != null) Destroy(view.gameObject);
            foreach (var view in _vertexViews) if (view != null) Destroy(view.gameObject);
            foreach (var view in _edgeViews) if (view != null) Destroy(view.gameObject);
            _tileViews.Clear();
            _vertexViews.Clear();
            _edgeViews.Clear();
        }

        // ── Coordinate math ────────────────────────────────────────────────────

        private Vector3 HexToWorld(GameCore.Board.HexCoord coord)
        {
            float x = HexSize * (1.5f * coord.Q);
            float y = HexSize * (Mathf.Sqrt(3) * (coord.R + coord.Q * 0.5f));
            return new Vector3(x, y, 0f);
        }

        // Returns the world position of corner `cornerIndex` of the hex at `tileCoord`.
        // For flat-top hexes, corner 0 is at the right (0°), then every 60° clockwise:
        //   0 = right, 1 = lower-right, 2 = lower-left, 3 = left, 4 = upper-left, 5 = upper-right.
        // This is correct for every vertex — interior and border alike — because it uses
        // only the tile center and the known hex geometry, not neighbouring tile positions.
        private Vector3 CornerWorldPosition(GameCore.Board.HexCoord tileCoord, int cornerIndex)
        {
            var center = HexToWorld(tileCoord);
            float angleRad = -60f * cornerIndex * Mathf.Deg2Rad;
            return center + new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f) * HexSize;
        }

        private Sprite GetTileSprite(CatanResourceType? resourceType)
        {
            if (resourceType == null) return DesertSprite;
            int index = (int)resourceType.Value;
            if (TileSprites == null || index < 0 || index >= TileSprites.Length) return null;
            return TileSprites[index];
        }
    }
}
