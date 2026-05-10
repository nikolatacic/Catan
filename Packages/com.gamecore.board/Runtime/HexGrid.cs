using System;
using System.Collections.Generic;

namespace GameCore.Board
{
    public class HexGrid<T> where T : IHexTile
    {
        private readonly Dictionary<HexCoord, T> _tiles = new();
        private readonly Dictionary<HexCoord, List<HexVertex>> _tileToVertices = new();
        private readonly Dictionary<HexCoord, List<HexEdge>> _tileToEdges = new();

        public IReadOnlyDictionary<HexCoord, T> Tiles => _tiles;

        public T GetTile(HexCoord coord) => _tiles.TryGetValue(coord, out var tile) ? tile : default;
        public void SetTile(HexCoord coord, T tile) => _tiles[coord] = tile;

        public List<T> GetNeighbors(HexCoord coord)
        {
            var neighborList = new List<T>(6);
            foreach (var direction in HexCoord.Directions)
            {
                var neighborCoord = new HexCoord(coord.Q + direction.Q, coord.R + direction.R);
                if (_tiles.TryGetValue(neighborCoord, out var neighborTile))
                    neighborList.Add(neighborTile);
            }
            return neighborList;
        }

        public List<HexVertex> GetVertices(HexCoord coord)
        {
            if (_tileToVertices.TryGetValue(coord, out var vertices)) return vertices;
            throw new InvalidOperationException(
                "BuildTopology() must be called before querying vertices.");
        }

        public List<HexEdge> GetEdges(HexCoord coord)
        {
            if (_tileToEdges.TryGetValue(coord, out var edges)) return edges;
            throw new InvalidOperationException(
                "BuildTopology() must be called before querying edges.");
        }

        public void BuildTopology()
        {
            _tileToVertices.Clear();
            _tileToEdges.Clear();

            var edgeByKey = new Dictionary<(HexCoord, HexCoord), HexEdge>();
            var vertexByKey = new Dictionary<(HexCoord, HexCoord, HexCoord), HexVertex>();
            var edgesPerVertexKey = new Dictionary<(HexCoord, HexCoord, HexCoord), List<HexEdge>>();

            // Pass 1 — create all unique edges and vertices
            foreach (var tileCoord in _tiles.Keys)
            {
                for (int directionIndex = 0; directionIndex < 6; directionIndex++)
                {
                    var neighborCoord = NeighborInDirection(tileCoord, directionIndex);
                    var nextNeighborCoord = NeighborInDirection(tileCoord, (directionIndex + 1) % 6);

                    var edgeKey = MakeEdgeKey(tileCoord, neighborCoord);
                    if (!edgeByKey.ContainsKey(edgeKey))
                    {
                        var adjacentTileCoords = BuildAdjacentTileList(tileCoord, neighborCoord);
                        edgeByKey[edgeKey] = new HexEdge(adjacentTileCoords, Array.Empty<HexVertex>());
                    }

                    var vertexKey = MakeVertexKey(tileCoord, neighborCoord, nextNeighborCoord);
                    if (!vertexByKey.ContainsKey(vertexKey))
                    {
                        var adjacentTileCoords = BuildAdjacentTileList(tileCoord, neighborCoord, nextNeighborCoord);
                        vertexByKey[vertexKey] = new HexVertex(adjacentTileCoords);
                        edgesPerVertexKey[vertexKey] = new List<HexEdge>();
                    }
                }
            }

            // Pass 2 — wire each edge to its two adjacent vertices; collect edges per vertex
            foreach (var tileCoord in _tiles.Keys)
            {
                for (int directionIndex = 0; directionIndex < 6; directionIndex++)
                {
                    var neighborCoord = NeighborInDirection(tileCoord, directionIndex);
                    var prevNeighborCoord = NeighborInDirection(tileCoord, (directionIndex + 5) % 6);
                    var nextNeighborCoord = NeighborInDirection(tileCoord, (directionIndex + 1) % 6);

                    var edgeKey = MakeEdgeKey(tileCoord, neighborCoord);
                    var edge = edgeByKey[edgeKey];

                    var vertexKeyA = MakeVertexKey(tileCoord, prevNeighborCoord, neighborCoord);
                    var vertexKeyB = MakeVertexKey(tileCoord, neighborCoord, nextNeighborCoord);

                    if (edge.AdjacentVertices.Length == 0)
                        edge.AdjacentVertices = new[] { vertexByKey[vertexKeyA], vertexByKey[vertexKeyB] };

                    var edgeListForVertexA = edgesPerVertexKey[vertexKeyA];
                    if (!edgeListForVertexA.Contains(edge))
                        edgeListForVertexA.Add(edge);

                    var edgeListForVertexB = edgesPerVertexKey[vertexKeyB];
                    if (!edgeListForVertexB.Contains(edge))
                        edgeListForVertexB.Add(edge);
                }
            }

            // Assign collected edge lists to each vertex
            foreach (var (vertexKey, edgeList) in edgesPerVertexKey)
                vertexByKey[vertexKey].AdjacentEdges = edgeList.ToArray();

            // Pass 3 — build per-tile vertex and edge lookup lists
            foreach (var tileCoord in _tiles.Keys)
            {
                var tileVertexList = new List<HexVertex>(6);
                var tileEdgeList = new List<HexEdge>(6);

                for (int directionIndex = 0; directionIndex < 6; directionIndex++)
                {
                    var neighborCoord = NeighborInDirection(tileCoord, directionIndex);
                    var nextNeighborCoord = NeighborInDirection(tileCoord, (directionIndex + 1) % 6);

                    tileEdgeList.Add(edgeByKey[MakeEdgeKey(tileCoord, neighborCoord)]);
                    tileVertexList.Add(vertexByKey[MakeVertexKey(tileCoord, neighborCoord, nextNeighborCoord)]);
                }

                _tileToVertices[tileCoord] = tileVertexList;
                _tileToEdges[tileCoord] = tileEdgeList;
            }
        }

        private HexCoord NeighborInDirection(HexCoord tileCoord, int directionIndex)
        {
            var direction = HexCoord.Directions[directionIndex];
            return new HexCoord(tileCoord.Q + direction.Q, tileCoord.R + direction.R);
        }

        private HexCoord[] BuildAdjacentTileList(HexCoord firstCoord, HexCoord secondCoord)
        {
            var adjacentList = new List<HexCoord>(2) { firstCoord };
            if (_tiles.ContainsKey(secondCoord)) adjacentList.Add(secondCoord);
            return adjacentList.ToArray();
        }

        private HexCoord[] BuildAdjacentTileList(HexCoord firstCoord, HexCoord secondCoord, HexCoord thirdCoord)
        {
            var adjacentList = new List<HexCoord>(3) { firstCoord };
            if (_tiles.ContainsKey(secondCoord)) adjacentList.Add(secondCoord);
            if (_tiles.ContainsKey(thirdCoord)) adjacentList.Add(thirdCoord);
            return adjacentList.ToArray();
        }

        private static (HexCoord, HexCoord) MakeEdgeKey(HexCoord coordA, HexCoord coordB)
        {
            bool firstIsSmaller = coordA.Q < coordB.Q || (coordA.Q == coordB.Q && coordA.R < coordB.R);
            return firstIsSmaller ? (coordA, coordB) : (coordB, coordA);
        }

        private static (HexCoord, HexCoord, HexCoord) MakeVertexKey(
            HexCoord coordA, HexCoord coordB, HexCoord coordC)
        {
            var sortedCoords = new[] { coordA, coordB, coordC };
            Array.Sort(sortedCoords, (firstCoord, secondCoord) =>
                firstCoord.Q != secondCoord.Q
                    ? firstCoord.Q.CompareTo(secondCoord.Q)
                    : firstCoord.R.CompareTo(secondCoord.R));
            return (sortedCoords[0], sortedCoords[1], sortedCoords[2]);
        }
    }
}
