using System.Collections.Generic;
using NUnit.Framework;
using GameCore.Events;
using GameCore.Resources;
using GameCore.Score;
using UnityEngine;

namespace Catan.Tests
{
    public class ScoreManagerTests
    {
        private class TestResource : IResource
        {
            public string ResourceId { get; }
            public string DisplayName { get; }
            public TestResource(string resourceId) { ResourceId = resourceId; DisplayName = resourceId; }
        }

        private CatanBoard _board;
        private CatanPlayer _playerRed;
        private CatanPlayer _playerBlue;
        private GameObject _gameObject;
        private ScoreManager _scoreManager;
        private CatanVictoryCondition _victoryCondition;

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
            _playerRed = new CatanPlayer("red", "Red", Color.red);
            _playerBlue = new CatanPlayer("blue", "Blue", Color.blue);

            _gameObject = new GameObject("ScoreManagerTest");
            _scoreManager = _gameObject.AddComponent<ScoreManager>();
            _scoreManager.Players.Add(_playerRed);
            _scoreManager.Players.Add(_playerBlue);

            _victoryCondition = new CatanVictoryCondition(_board);
            _scoreManager.Conditions.Add(_victoryCondition);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_gameObject);
        }

        // ── GetScore ───────────────────────────────────────────────────────────

        [Test]
        public void GetScore_NoSettlements_ReturnsZero()
        {
            Assert.AreEqual(0, _scoreManager.GetScore(_playerRed));
        }

        [Test]
        public void GetScore_OneSettlement_ReturnsOne()
        {
            PlaceSettlement(_playerRed, 0);
            Assert.AreEqual(1, _scoreManager.GetScore(_playerRed));
        }

        [Test]
        public void GetScore_OneCity_ReturnsTwo()
        {
            PlaceCity(_playerRed, 0);
            Assert.AreEqual(2, _scoreManager.GetScore(_playerRed));
        }

        [Test]
        public void GetScore_LargestArmy_AddsTwoPoints()
        {
            _playerRed.HasLargestArmy = true;
            Assert.AreEqual(2, _scoreManager.GetScore(_playerRed));
        }

        [Test]
        public void GetScore_LongestRoad_AddsTwoPoints()
        {
            _playerRed.HasLongestRoad = true;
            Assert.AreEqual(2, _scoreManager.GetScore(_playerRed));
        }

        [Test]
        public void GetScore_VictoryPointCard_AddsOnePoint()
        {
            var vpCard = new VictoryPointCard();
            _playerRed.DevelopmentCards.Add(vpCard);
            Assert.AreEqual(1, _scoreManager.GetScore(_playerRed));
        }

        // ── GetLeader ──────────────────────────────────────────────────────────

        [Test]
        public void GetLeader_ReturnsPlayerWithHighestScore()
        {
            PlaceSettlement(_playerRed, 0);
            PlaceSettlement(_playerBlue, 1);
            PlaceSettlement(_playerBlue, 2); // blue has 2, red has 1
            Assert.AreEqual(_playerBlue, _scoreManager.GetLeader());
        }

        [Test]
        public void GetLeader_TiedScore_ReturnsFirstPlayer()
        {
            PlaceSettlement(_playerRed, 0);
            PlaceSettlement(_playerBlue, 1);
            // Both have 1 point — red is first in Players list
            Assert.AreEqual(_playerRed, _scoreManager.GetLeader());
        }

        // ── CheckVictory ───────────────────────────────────────────────────────

        [Test]
        public void CheckVictory_NoWinner_ReturnsNull()
        {
            Assert.IsNull(_scoreManager.CheckVictory());
        }

        [Test]
        public void CheckVictory_PlayerAtTenPoints_ReturnsWinner()
        {
            _victoryCondition.TargetPoints = 2;
            PlaceSettlement(_playerRed, 0);
            PlaceSettlement(_playerRed, 1);

            var winner = _scoreManager.CheckVictory();
            Assert.AreEqual(_playerRed, winner);
        }

        [Test]
        public void CheckVictory_Winner_PublishesVictoryAchievedEvent()
        {
            _victoryCondition.TargetPoints = 1;
            PlaceSettlement(_playerRed, 0);

            VictoryAchievedEvent? receivedEvent = null;
            EventBus.Subscribe<VictoryAchievedEvent>(gameEvent => receivedEvent = gameEvent);

            _scoreManager.CheckVictory();

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(_playerRed, receivedEvent!.Value.Winner);
        }

        // ── RecalculateAll ─────────────────────────────────────────────────────

        [Test]
        public void RecalculateAll_ScoreChanges_PublishesScoreChangedEvent()
        {
            ScoreChangedEvent? receivedEvent = null;
            EventBus.Subscribe<ScoreChangedEvent>(gameEvent => receivedEvent = gameEvent);

            PlaceSettlement(_playerRed, 0);
            _scoreManager.RecalculateAll();

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(_playerRed, receivedEvent!.Value.Player);
            Assert.AreEqual(0, receivedEvent!.Value.OldScore);
            Assert.AreEqual(1, receivedEvent!.Value.NewScore);
        }

        [Test]
        public void RecalculateAll_NoScoreChange_DoesNotPublishScoreChangedEvent()
        {
            _scoreManager.RecalculateAll(); // initial call to set baseline

            bool eventFired = false;
            EventBus.Subscribe<ScoreChangedEvent>(_ => eventFired = true);

            _scoreManager.RecalculateAll(); // nothing changed

            Assert.IsFalse(eventFired);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void PlaceSettlement(CatanPlayer player, int vertexIndex)
        {
            var allVertices = new System.Collections.Generic.List<GameCore.Board.HexVertex>();
            foreach (var tile in _board.Grid.Tiles.Values)
                foreach (var vertex in _board.Grid.GetVertices(tile.Coord))
                    if (!allVertices.Contains(vertex)) allVertices.Add(vertex);

            if (vertexIndex >= allVertices.Count) return;
            var targetVertex = allVertices[vertexIndex];
            if (_board.Settlements.ContainsKey(targetVertex)) return;

            var settlement = new Settlement(player, targetVertex);
            _board.Settlements[targetVertex] = settlement;
            player.Settlements.Add(settlement);
        }

        private void PlaceCity(CatanPlayer player, int vertexIndex)
        {
            PlaceSettlement(player, vertexIndex);
            var settlement = player.Settlements[player.Settlements.Count - 1];
            settlement.UpgradeToCity();
        }
    }
}
