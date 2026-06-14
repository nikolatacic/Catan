using System.Collections.Generic;
using UnityEngine;

namespace Catan.UI
{
    // ── Editor wiring required ─────────────────────────────────────────────────
    // Attach to a child GameObject of GameManager ("BoardRenderer").
    // Assign HexTilePrefab, VertexPrefab, EdgePrefab.
    // Assign TileSprites array indexed by CatanTileType enum value.
    // Call RenderBoard() after GameManager initializes the Board.
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

                foreach (var vertex in board.Grid.GetVertices(tile.Coord))
                {
                    if (!spawnedVertices.Add(vertex)) continue;
                    SpawnVertex(vertex, tile.Coord);
                }

                foreach (var edge in board.Grid.GetEdges(tile.Coord))
                {
                    if (!spawnedEdges.Add(edge)) continue;
                    SpawnEdge(edge, tile.Coord);
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

            var sprite = GetTileSprite(tile.ResourceType);
            tileView.Initialize(tile, sprite);
            _tileViews.Add(tileView);
        }

        private void SpawnVertex(GameCore.Board.HexVertex vertex, GameCore.Board.HexCoord tileCoord)
        {
            if (VertexPrefab == null) return;

            var worldPos = VertexWorldPosition(vertex, tileCoord);
            var go = Instantiate(VertexPrefab, worldPos, Quaternion.identity, transform);
            go.name = $"Vertex_{vertex.GetHashCode()}";

            var vertexView = go.GetComponent<VertexView>();
            if (vertexView == null) return;

            vertexView.Initialize(vertex);
            _vertexViews.Add(vertexView);
        }

        private void SpawnEdge(GameCore.Board.HexEdge edge, GameCore.Board.HexCoord tileCoord)
        {
            if (EdgePrefab == null) return;

            if (edge.AdjacentVertices == null || edge.AdjacentVertices.Length < 2) return;

            var worldPos = EdgeWorldPosition(edge, tileCoord);
            var go = Instantiate(EdgePrefab, worldPos, Quaternion.identity, transform);
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

        private Vector3 VertexWorldPosition(GameCore.Board.HexVertex vertex, GameCore.Board.HexCoord tileCoord)
        {
            // Average the world positions of all tiles adjacent to this vertex,
            // then push outward from the primary tile center in that direction.
            if (vertex.AdjacentTiles == null || vertex.AdjacentTiles.Length == 0)
                return HexToWorld(tileCoord);

            var centroid = Vector3.zero;
            foreach (var adjacentCoord in vertex.AdjacentTiles)
                centroid += HexToWorld(adjacentCoord);
            centroid /= vertex.AdjacentTiles.Length;

            // The vertex sits at the centroid of its adjacent tile centres,
            // which is exactly the hex corner shared by those tiles.
            return centroid;
        }

        private Vector3 EdgeWorldPosition(GameCore.Board.HexEdge edge, GameCore.Board.HexCoord tileCoord)
        {
            if (edge.AdjacentVertices == null || edge.AdjacentVertices.Length < 2)
                return HexToWorld(tileCoord);

            var posA = VertexWorldPosition(edge.AdjacentVertices[0], tileCoord);
            var posB = VertexWorldPosition(edge.AdjacentVertices[1], tileCoord);
            return (posA + posB) * 0.5f; // midpoint of the two endpoint vertices
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
