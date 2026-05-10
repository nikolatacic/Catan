using NUnit.Framework;
using GameCore.Events;
using GameCore.Resources;
using UnityEngine;

namespace Catan.Tests
{
    public class LongestRoadTrackerTests
    {
        private class TestResource : IResource
        {
            public string ResourceId { get; }
            public string DisplayName { get; }
            public TestResource(string resourceId) { ResourceId = resourceId; DisplayName = resourceId; }
        }

        private CatanBoard _board;
        private LongestRoadTracker _tracker;
        private CatanPlayer _playerRed;
        private CatanPlayer _playerBlue;

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
            _tracker = new LongestRoadTracker();
            _playerRed = new CatanPlayer("red", "Red", Color.red);
            _playerBlue = new CatanPlayer("blue", "Blue", Color.blue);
        }

        [TearDown]
        public void TearDown() => EventBus.Clear();

        // ── No roads ───────────────────────────────────────────────────────────

        [Test]
        public void Recalculate_NoRoads_NoTokenAwarded()
        {
            _tracker.Recalculate(_board);
            Assert.IsNull(_tracker.CurrentHolder);
        }

        // ── Below minimum ──────────────────────────────────────────────────────

        [Test]
        public void Recalculate_FourRoads_TokenNotAwarded()
        {
            PlaceLinearRoads(_playerRed, 4);
            _tracker.Recalculate(_board);
            Assert.IsNull(_tracker.CurrentHolder);
        }

        // ── First player to five gets the token ───────────────────────────────

        [Test]
        public void Recalculate_FiveRoads_TokenAwarded()
        {
            PlaceLinearRoads(_playerRed, 5);
            _tracker.Recalculate(_board);
            Assert.AreEqual(_playerRed, _tracker.CurrentHolder);
            Assert.IsTrue(_playerRed.HasLongestRoad);
        }

        [Test]
        public void Recalculate_FiveRoads_PublishesLongestRoadChangedEvent()
        {
            PlaceLinearRoads(_playerRed, 5);

            LongestRoadChangedEvent? receivedEvent = null;
            EventBus.Subscribe<LongestRoadChangedEvent>(gameEvent => receivedEvent = gameEvent);

            _tracker.Recalculate(_board);

            Assert.IsNotNull(receivedEvent);
            Assert.IsNull(receivedEvent!.Value.Previous);
            Assert.AreEqual(_playerRed, receivedEvent!.Value.Current);
        }

        // ── Longer road takes the token ────────────────────────────────────────

        [Test]
        public void Recalculate_SecondPlayerExceedsCurrentHolder_TokenTransfers()
        {
            PlaceLinearRoads(_playerRed, 5);
            _tracker.Recalculate(_board);

            // Give blue a longer road using different edges
            PlaceLinearRoads(_playerBlue, 6);
            _tracker.Recalculate(_board);

            Assert.AreEqual(_playerBlue, _tracker.CurrentHolder);
            Assert.IsFalse(_playerRed.HasLongestRoad);
            Assert.IsTrue(_playerBlue.HasLongestRoad);
        }

        [Test]
        public void Recalculate_TiedLength_TokenDoesNotTransfer()
        {
            PlaceLinearRoads(_playerRed, 5);
            _tracker.Recalculate(_board);

            PlaceLinearRoads(_playerBlue, 5);
            _tracker.Recalculate(_board);

            // Red keeps the token on tie
            Assert.AreEqual(_playerRed, _tracker.CurrentHolder);
        }

        // ── No duplicate events ────────────────────────────────────────────────

        [Test]
        public void Recalculate_SameHolderSameLength_NoEventPublished()
        {
            PlaceLinearRoads(_playerRed, 5);
            _tracker.Recalculate(_board);

            bool eventPublished = false;
            EventBus.Subscribe<LongestRoadChangedEvent>(_ => eventPublished = true);

            _tracker.Recalculate(_board); // nothing changed

            Assert.IsFalse(eventPublished);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        // Places roads in a linear chain along the ring-1 tile edges.
        private void PlaceLinearRoads(CatanPlayer player, int count)
        {
            int placedCount = 0;
            foreach (var tile in _board.Grid.Tiles.Values)
            {
                var edges = _board.Grid.GetEdges(tile.Coord);
                foreach (var edge in edges)
                {
                    if (_board.Roads.ContainsKey(edge)) continue;

                    var road = new Road(player, edge);
                    _board.Roads[edge] = road;
                    player.Roads.Add(road);
                    placedCount++;

                    if (placedCount >= count) return;
                }
            }
        }
    }
}
