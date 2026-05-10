using System.Linq;
using NUnit.Framework;
using GameCore.Resources;
using UnityEngine;

namespace Catan.Tests
{
    public class PortSystemTests
    {
        private class TestResource : IResource
        {
            public string ResourceId { get; }
            public string DisplayName { get; }
            public TestResource(string resourceId) { ResourceId = resourceId; DisplayName = resourceId; }
        }

        private CatanBoard _board;
        private CatanPlayer _player;

        [SetUp]
        public void SetUp()
        {
            CatanResources.Initialize(
                new TestResource("wood"),
                new TestResource("brick"),
                new TestResource("sheep"),
                new TestResource("wheat"),
                new TestResource("ore"));

            var generator = new CatanBoardGenerator(new System.Random(42));
            _board = generator.GenerateBoard();
            _player = new CatanPlayer("red", "Red", Color.red);
        }

        // ── Default ratio ──────────────────────────────────────────────────────

        [Test]
        public void GetTradeRatio_NoSettlements_ReturnsFour()
        {
            var ratio = _board.Ports.GetTradeRatio(_player, CatanResourceType.Wood);
            Assert.AreEqual(4, ratio);
        }

        [Test]
        public void GetTradeRatio_SettlementNotOnPort_ReturnsFour()
        {
            var nonPortVertex = FindNonPortVertex();
            if (nonPortVertex == null) Assert.Ignore("No non-port vertex found with seed 42.");

            var settlement = new Settlement(_player, nonPortVertex);
            _player.Settlements.Add(settlement);

            var ratio = _board.Ports.GetTradeRatio(_player, CatanResourceType.Wood);
            Assert.AreEqual(4, ratio);
        }

        // ── Generic port ───────────────────────────────────────────────────────

        [Test]
        public void GetTradeRatio_SettlementOnGenericPort_ReturnsThree()
        {
            var genericPort = _board.Ports.Ports.FirstOrDefault(port => !port.SpecificResource.HasValue);
            if (genericPort == null) Assert.Ignore("No generic port found.");

            PlaceSettlementAt(genericPort.AccessVertices[0]);

            var ratio = _board.Ports.GetTradeRatio(_player, CatanResourceType.Ore);
            Assert.AreEqual(3, ratio);
        }

        [Test]
        public void GetTradeRatio_SettlementOnGenericPort_AppliesToAllResources()
        {
            var genericPort = _board.Ports.Ports.FirstOrDefault(port => !port.SpecificResource.HasValue);
            if (genericPort == null) Assert.Ignore("No generic port found.");

            PlaceSettlementAt(genericPort.AccessVertices[0]);

            foreach (var resourceType in System.Enum.GetValues(typeof(CatanResourceType)))
                Assert.AreEqual(3, _board.Ports.GetTradeRatio(_player, (CatanResourceType)resourceType));
        }

        // ── Specific port ──────────────────────────────────────────────────────

        [Test]
        public void GetTradeRatio_SettlementOnSpecificPort_ReturnsTwoForThatResource()
        {
            var woodPort = _board.Ports.Ports.FirstOrDefault(port => port.SpecificResource == CatanResourceType.Wood);
            if (woodPort == null) Assert.Ignore("No wood port found.");

            PlaceSettlementAt(woodPort.AccessVertices[0]);

            Assert.AreEqual(2, _board.Ports.GetTradeRatio(_player, CatanResourceType.Wood));
        }

        [Test]
        public void GetTradeRatio_SettlementOnSpecificPort_ReturnsFourForOtherResources()
        {
            var woodPort = _board.Ports.Ports.FirstOrDefault(port => port.SpecificResource == CatanResourceType.Wood);
            if (woodPort == null) Assert.Ignore("No wood port found.");

            PlaceSettlementAt(woodPort.AccessVertices[0]);

            Assert.AreEqual(4, _board.Ports.GetTradeRatio(_player, CatanResourceType.Brick));
            Assert.AreEqual(4, _board.Ports.GetTradeRatio(_player, CatanResourceType.Ore));
        }

        // ── Best ratio wins ────────────────────────────────────────────────────

        [Test]
        public void GetTradeRatio_BothSpecificAndGenericPort_ReturnsBestRatio()
        {
            var woodPort = _board.Ports.Ports.FirstOrDefault(port => port.SpecificResource == CatanResourceType.Wood);
            var genericPort = _board.Ports.Ports.FirstOrDefault(port => !port.SpecificResource.HasValue);
            if (woodPort == null || genericPort == null) Assert.Ignore("Required ports not found.");

            PlaceSettlementAt(woodPort.AccessVertices[0]);
            PlaceSettlementAt(genericPort.AccessVertices[0]);

            Assert.AreEqual(2, _board.Ports.GetTradeRatio(_player, CatanResourceType.Wood));
            Assert.AreEqual(3, _board.Ports.GetTradeRatio(_player, CatanResourceType.Brick));
        }

        // ── Access vertex coverage ─────────────────────────────────────────────

        [Test]
        public void GetTradeRatio_SettlementOnSecondAccessVertex_StillGrantsPortAccess()
        {
            var genericPort = _board.Ports.Ports.FirstOrDefault(port => !port.SpecificResource.HasValue);
            if (genericPort == null) Assert.Ignore("No generic port found.");

            PlaceSettlementAt(genericPort.AccessVertices[1]);

            Assert.AreEqual(3, _board.Ports.GetTradeRatio(_player, CatanResourceType.Sheep));
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void PlaceSettlementAt(GameCore.Board.HexVertex vertex)
        {
            var settlement = new Settlement(_player, vertex);
            _player.Settlements.Add(settlement);
            _board.Settlements[vertex] = settlement;
        }

        private GameCore.Board.HexVertex FindNonPortVertex()
        {
            var portVertices = new System.Collections.Generic.HashSet<GameCore.Board.HexVertex>(
                _board.Ports.Ports.SelectMany(port => port.AccessVertices));

            foreach (var tile in _board.Grid.Tiles.Values)
            {
                foreach (var vertex in _board.Grid.GetVertices(tile.Coord))
                {
                    if (!portVertices.Contains(vertex))
                        return vertex;
                }
            }

            return null;
        }
    }
}
