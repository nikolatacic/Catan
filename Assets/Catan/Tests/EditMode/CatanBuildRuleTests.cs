using NUnit.Framework;
using GameCore.Events;
using GameCore.Resources;
using UnityEngine;

namespace Catan.Tests
{
    public class CatanBuildRuleTests
    {
        private class TestResource : IResource
        {
            public string ResourceId { get; }
            public string DisplayName { get; }
            public TestResource(string resourceId) { ResourceId = resourceId; DisplayName = resourceId; }
        }

        private CatanBoard _board;
        private CatanPlayer _player;
        private CatanBuildRule _rule;
        private GameObject _gameObject;
        private CatanTurnManager _turnManager;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            CatanResources.Initialize(
                new TestResource("wood"), new TestResource("brick"),
                new TestResource("sheep"), new TestResource("wheat"),
                new TestResource("ore"));

            var generator = new CatanBoardGenerator(new System.Random(1));
            _board = generator.GenerateBoard();
            _player = new CatanPlayer("red", "Red", Color.red);

            _gameObject = new GameObject("TurnManagerTest");
            _turnManager = _gameObject.AddComponent<CatanTurnManager>();
            _turnManager.Actors.Add(_player);
            _turnManager.Initialize(_board);

            _rule = new CatanBuildRule(_board, _turnManager);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_gameObject);
        }

        // ── Settlement placement: setup phase (no resources required) ──────────

        [Test]
        public void CanPlace_Settlement_DuringSetup_EmptyVertex_ReturnsTrue()
        {
            _turnManager.StartGame(); // → SetupPlacement
            var vertex = FreeVertex();
            var settlement = new Settlement(_player, vertex);
            Assert.IsTrue(_rule.CanPlace(settlement, settlement, _player));
        }

        [Test]
        public void CanPlace_Settlement_DuringSetup_OccupiedVertex_ReturnsFalse()
        {
            _turnManager.StartGame();
            var vertex = FreeVertex();
            _board.Settlements[vertex] = new Settlement(_player, vertex);
            var newSettlement = new Settlement(_player, vertex);
            Assert.IsFalse(_rule.CanPlace(newSettlement, newSettlement, _player));
        }

        [Test]
        public void CanPlace_Settlement_DuringSetup_DistanceRuleViolated_ReturnsFalse()
        {
            _turnManager.StartGame();
            var vertex = FreeVertex();
            _board.Settlements[vertex] = new Settlement(_player, vertex);

            // Adjacent vertex
            var adjacentVertex = AdjacentVertex(vertex);
            if (adjacentVertex == null) return;

            var settlement = new Settlement(_player, adjacentVertex);
            Assert.IsFalse(_rule.CanPlace(settlement, settlement, _player));
        }

        // ── Settlement placement: normal play (resources + road required) ──────

        [Test]
        public void CanPlace_Settlement_NormalPlay_NoResources_ReturnsFalse()
        {
            _turnManager.HandleDiceRoll(6); // → Trading (normal play)
            var vertex = FreeVertex();
            PlaceOwnRoadAdjacentTo(vertex);
            var settlement = new Settlement(_player, vertex);
            Assert.IsFalse(_rule.CanPlace(settlement, settlement, _player));
        }

        [Test]
        public void CanPlace_Settlement_NormalPlay_NoRoadConnection_ReturnsFalse()
        {
            _turnManager.HandleDiceRoll(6);
            GiveSettlementResources();
            var vertex = FreeVertex();
            var settlement = new Settlement(_player, vertex);
            Assert.IsFalse(_rule.CanPlace(settlement, settlement, _player));
        }

        [Test]
        public void CanPlace_Settlement_NormalPlay_AllConditionsMet_ReturnsTrue()
        {
            _turnManager.HandleDiceRoll(6);
            GiveSettlementResources();
            var vertex = FreeVertex();
            PlaceOwnRoadAdjacentTo(vertex);
            var settlement = new Settlement(_player, vertex);
            Assert.IsTrue(_rule.CanPlace(settlement, settlement, _player));
        }

        // ── City upgrade ───────────────────────────────────────────────────────

        [Test]
        public void CanPlace_City_OwnSettlementAtVertex_HasResources_ReturnsTrue()
        {
            var vertex = FreeVertex();
            var settlement = new Settlement(_player, vertex);
            _board.Settlements[vertex] = settlement;
            _player.Settlements.Add(settlement);
            GiveCityResources();
            var upgrade = new CityUpgrade(_player, vertex);
            Assert.IsTrue(_rule.CanPlace(upgrade, upgrade, _player));
        }

        [Test]
        public void CanPlace_City_NoSettlementAtVertex_ReturnsFalse()
        {
            var vertex = FreeVertex();
            GiveCityResources();
            var upgrade = new CityUpgrade(_player, vertex);
            Assert.IsFalse(_rule.CanPlace(upgrade, upgrade, _player));
        }

        // ── Road placement ─────────────────────────────────────────────────────

        [Test]
        public void CanPlace_Road_DuringSetup_ConnectedToOwnSettlement_ReturnsTrue()
        {
            _turnManager.StartGame();
            var vertex = FreeVertex();
            _board.Settlements[vertex] = new Settlement(_player, vertex);

            var edge = EdgeAdjacentTo(vertex);
            if (edge == null) return;

            var road = new Road(_player, edge);
            Assert.IsTrue(_rule.CanPlace(road, road, _player));
        }

        [Test]
        public void CanPlace_Road_NormalPlay_NoConnection_ReturnsFalse()
        {
            _turnManager.HandleDiceRoll(6);
            GiveRoadResources();
            var edge = FreeEdge();
            var road = new Road(_player, edge);
            Assert.IsFalse(_rule.CanPlace(road, road, _player));
        }

        [Test]
        public void CanPlace_Road_OccupiedEdge_ReturnsFalse()
        {
            _turnManager.StartGame();
            var vertex = FreeVertex();
            _board.Settlements[vertex] = new Settlement(_player, vertex);
            var edge = EdgeAdjacentTo(vertex);
            if (edge == null) return;

            _board.Roads[edge] = new Road(_player, edge);
            var newRoad = new Road(_player, edge);
            Assert.IsFalse(_rule.CanPlace(newRoad, newRoad, _player));
        }

        // ── CanRemove: always false ────────────────────────────────────────────

        [Test]
        public void CanRemove_AlwaysReturnsFalse()
        {
            var vertex = FreeVertex();
            var settlement = new Settlement(_player, vertex);
            Assert.IsFalse(_rule.CanRemove(settlement, settlement, _player));
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private GameCore.Board.HexVertex FreeVertex()
        {
            foreach (var tile in _board.Grid.Tiles.Values)
                foreach (var vertex in _board.Grid.GetVertices(tile.Coord))
                    if (!_board.Settlements.ContainsKey(vertex)) return vertex;
            return null;
        }

        private GameCore.Board.HexVertex AdjacentVertex(GameCore.Board.HexVertex vertex)
        {
            if (vertex.AdjacentEdges == null) return null;
            foreach (var edge in vertex.AdjacentEdges)
                foreach (var adjacentVertex in edge.AdjacentVertices)
                    if (adjacentVertex != vertex) return adjacentVertex;
            return null;
        }

        private GameCore.Board.HexEdge FreeEdge()
        {
            foreach (var tile in _board.Grid.Tiles.Values)
                foreach (var edge in _board.Grid.GetEdges(tile.Coord))
                    if (!_board.Roads.ContainsKey(edge)) return edge;
            return null;
        }

        private GameCore.Board.HexEdge EdgeAdjacentTo(GameCore.Board.HexVertex vertex)
        {
            if (vertex.AdjacentEdges == null) return null;
            foreach (var edge in vertex.AdjacentEdges)
                if (!_board.Roads.ContainsKey(edge)) return edge;
            return null;
        }

        private void PlaceOwnRoadAdjacentTo(GameCore.Board.HexVertex vertex)
        {
            var edge = EdgeAdjacentTo(vertex);
            if (edge == null) return;
            _board.Roads[edge] = new Road(_player, edge);
        }

        private void GiveSettlementResources()
        {
            _player.Resources.TryAdd(new ResourceBundle()
                .Add(CatanResources.Wood, 1)
                .Add(CatanResources.Brick, 1)
                .Add(CatanResources.Sheep, 1)
                .Add(CatanResources.Wheat, 1));
        }

        private void GiveCityResources()
        {
            _player.Resources.TryAdd(new ResourceBundle()
                .Add(CatanResources.Wheat, 2)
                .Add(CatanResources.Ore, 3));
        }

        private void GiveRoadResources()
        {
            _player.Resources.TryAdd(new ResourceBundle()
                .Add(CatanResources.Wood, 1)
                .Add(CatanResources.Brick, 1));
        }
    }
}
