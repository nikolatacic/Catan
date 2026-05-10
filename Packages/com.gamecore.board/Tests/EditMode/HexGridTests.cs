using System.Linq;
using NUnit.Framework;

namespace GameCore.Board.Tests
{
    public class HexGridTests
    {
        private class TestTile : IHexTile
        {
            public HexCoord Coord { get; }
            public TestTile(HexCoord coord) => Coord = coord;
        }

        private static HexGrid<TestTile> BuildSingleTileGrid(HexCoord coord)
        {
            var grid = new HexGrid<TestTile>();
            grid.SetTile(coord, new TestTile(coord));
            return grid;
        }

        private static HexGrid<TestTile> BuildTwoAdjacentTileGrid(HexCoord coordA, HexCoord coordB)
        {
            var grid = new HexGrid<TestTile>();
            grid.SetTile(coordA, new TestTile(coordA));
            grid.SetTile(coordB, new TestTile(coordB));
            return grid;
        }

        // ── GetVertices / GetEdges before BuildTopology ────────────────────────

        [Test]
        public void GetVertices_BeforeBuildTopology_ThrowsInvalidOperationException()
        {
            var grid = BuildSingleTileGrid(new HexCoord(0, 0));
            Assert.Throws<System.InvalidOperationException>(() => grid.GetVertices(new HexCoord(0, 0)));
        }

        [Test]
        public void GetEdges_BeforeBuildTopology_ThrowsInvalidOperationException()
        {
            var grid = BuildSingleTileGrid(new HexCoord(0, 0));
            Assert.Throws<System.InvalidOperationException>(() => grid.GetEdges(new HexCoord(0, 0)));
        }

        // ── Vertex and edge counts ─────────────────────────────────────────────

        [Test]
        public void GetVertices_AfterBuildTopology_ReturnsSixVertices()
        {
            var originCoord = new HexCoord(0, 0);
            var grid = BuildSingleTileGrid(originCoord);
            grid.BuildTopology();

            Assert.AreEqual(6, grid.GetVertices(originCoord).Count);
        }

        [Test]
        public void GetEdges_AfterBuildTopology_ReturnsSixEdges()
        {
            var originCoord = new HexCoord(0, 0);
            var grid = BuildSingleTileGrid(originCoord);
            grid.BuildTopology();

            Assert.AreEqual(6, grid.GetEdges(originCoord).Count);
        }

        [Test]
        public void GetVertices_NoNullEntries()
        {
            var originCoord = new HexCoord(0, 0);
            var grid = BuildSingleTileGrid(originCoord);
            grid.BuildTopology();

            foreach (var vertex in grid.GetVertices(originCoord))
                Assert.IsNotNull(vertex);
        }

        [Test]
        public void GetEdges_NoNullEntries()
        {
            var originCoord = new HexCoord(0, 0);
            var grid = BuildSingleTileGrid(originCoord);
            grid.BuildTopology();

            foreach (var edge in grid.GetEdges(originCoord))
                Assert.IsNotNull(edge);
        }

        // ── Sharing between adjacent tiles ─────────────────────────────────────

        [Test]
        public void AdjacentTiles_ShareExactlyTwoVertices()
        {
            var coordA = new HexCoord(0, 0);
            var coordB = new HexCoord(1, 0);
            var grid = BuildTwoAdjacentTileGrid(coordA, coordB);
            grid.BuildTopology();

            var verticesA = grid.GetVertices(coordA);
            var verticesB = grid.GetVertices(coordB);
            var sharedVertices = verticesA.Intersect(verticesB).ToList();

            Assert.AreEqual(2, sharedVertices.Count,
                "Adjacent hexes must share exactly 2 vertices (same object references).");
        }

        [Test]
        public void AdjacentTiles_ShareExactlyOneEdge()
        {
            var coordA = new HexCoord(0, 0);
            var coordB = new HexCoord(1, 0);
            var grid = BuildTwoAdjacentTileGrid(coordA, coordB);
            grid.BuildTopology();

            var edgesA = grid.GetEdges(coordA);
            var edgesB = grid.GetEdges(coordB);
            var sharedEdges = edgesA.Intersect(edgesB).ToList();

            Assert.AreEqual(1, sharedEdges.Count,
                "Adjacent hexes must share exactly 1 edge (same object reference).");
        }

        [Test]
        public void NonAdjacentTiles_ShareNoVertices()
        {
            var grid = new HexGrid<TestTile>();
            var coordA = new HexCoord(0, 0);
            var coordB = new HexCoord(3, 0);
            grid.SetTile(coordA, new TestTile(coordA));
            grid.SetTile(coordB, new TestTile(coordB));
            grid.BuildTopology();

            var verticesA = grid.GetVertices(coordA);
            var verticesB = grid.GetVertices(coordB);
            var sharedVertices = verticesA.Intersect(verticesB).ToList();

            Assert.AreEqual(0, sharedVertices.Count);
        }

        // ── Vertex adjacency wiring ────────────────────────────────────────────

        [Test]
        public void SharedEdge_HasTwoAdjacentVertices()
        {
            var coordA = new HexCoord(0, 0);
            var coordB = new HexCoord(1, 0);
            var grid = BuildTwoAdjacentTileGrid(coordA, coordB);
            grid.BuildTopology();

            var sharedEdge = grid.GetEdges(coordA).Intersect(grid.GetEdges(coordB)).First();

            Assert.AreEqual(2, sharedEdge.AdjacentVertices.Length);
        }

        [Test]
        public void InteriorVertex_InThreeTileGrid_HasThreeAdjacentTiles()
        {
            // Three hexes sharing a common vertex: (0,0), (1,0), (1,-1)
            var grid = new HexGrid<TestTile>();
            var coordA = new HexCoord(0, 0);
            var coordB = new HexCoord(1, 0);
            var coordC = new HexCoord(1, -1);
            grid.SetTile(coordA, new TestTile(coordA));
            grid.SetTile(coordB, new TestTile(coordB));
            grid.SetTile(coordC, new TestTile(coordC));
            grid.BuildTopology();

            // The shared vertex between all three should have 3 adjacent tiles
            var verticesA = grid.GetVertices(coordA);
            var verticesB = grid.GetVertices(coordB);
            var verticesC = grid.GetVertices(coordC);
            var sharedByAll = verticesA.Intersect(verticesB).Intersect(verticesC).ToList();

            Assert.AreEqual(1, sharedByAll.Count, "The three hexes should share exactly 1 vertex.");
            Assert.AreEqual(3, sharedByAll[0].AdjacentTiles.Length,
                "The shared vertex should list all 3 adjacent tile coords.");
        }

        [Test]
        public void BorderEdge_HasOneAdjacentTile()
        {
            var originCoord = new HexCoord(0, 0);
            var grid = BuildSingleTileGrid(originCoord);
            grid.BuildTopology();

            foreach (var edge in grid.GetEdges(originCoord))
                Assert.AreEqual(1, edge.AdjacentTiles.Length,
                    "All edges of an isolated tile should have exactly 1 adjacent tile.");
        }

        [Test]
        public void SharedEdge_HasTwoAdjacentTiles()
        {
            var coordA = new HexCoord(0, 0);
            var coordB = new HexCoord(1, 0);
            var grid = BuildTwoAdjacentTileGrid(coordA, coordB);
            grid.BuildTopology();

            var sharedEdge = grid.GetEdges(coordA).Intersect(grid.GetEdges(coordB)).First();

            Assert.AreEqual(2, sharedEdge.AdjacentTiles.Length);
        }

        // ── BuildTopology idempotency ──────────────────────────────────────────

        [Test]
        public void BuildTopology_CalledTwice_DoesNotDuplicateVertices()
        {
            var originCoord = new HexCoord(0, 0);
            var grid = BuildSingleTileGrid(originCoord);
            grid.BuildTopology();
            grid.BuildTopology();

            Assert.AreEqual(6, grid.GetVertices(originCoord).Count);
        }

        // ── GetNeighbors ───────────────────────────────────────────────────────

        [Test]
        public void GetNeighbors_ReturnsOnlyTilesInGrid()
        {
            var grid = BuildTwoAdjacentTileGrid(new HexCoord(0, 0), new HexCoord(1, 0));
            var neighbors = grid.GetNeighbors(new HexCoord(0, 0));

            Assert.AreEqual(1, neighbors.Count);
            Assert.AreEqual(new HexCoord(1, 0), neighbors[0].Coord);
        }
    }
}
