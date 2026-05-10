using System.Collections.Generic;
using NUnit.Framework;
using GameCore.Events;
using GameCore.Resources;
using UnityEngine;

namespace Catan.Tests
{
    public class RobberSystemTests
    {
        private class TestResource : IResource
        {
            public string ResourceId { get; }
            public string DisplayName { get; }
            public TestResource(string resourceId) { ResourceId = resourceId; DisplayName = resourceId; }
        }

        private CatanBoard _board;
        private CatanPlayer _activePlayer;
        private CatanPlayer _victimPlayer;
        private RobberSystem _robberSystem;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            CatanResources.Initialize(
                new TestResource("wood"),
                new TestResource("brick"),
                new TestResource("sheep"),
                new TestResource("wheat"),
                new TestResource("ore"));

            var generator = new CatanBoardGenerator(new System.Random(1));
            _board = generator.GenerateBoard();
            _activePlayer = new CatanPlayer("active", "Active", Color.red);
            _victimPlayer = new CatanPlayer("victim", "Victim", Color.blue);

            var allPlayers = new List<GameCore.Player.IPlayer> { _activePlayer, _victimPlayer };
            _robberSystem = new RobberSystem(new System.Random(0));
            _robberSystem.Initialize(_board, allPlayers);
        }

        [TearDown]
        public void TearDown() => EventBus.Clear();

        // ── Activate ───────────────────────────────────────────────────────────

        [Test]
        public void Activate_SetsMustMoveTrue()
        {
            _robberSystem.Activate(_activePlayer);
            Assert.IsTrue(_robberSystem.MustMove);
        }

        [Test]
        public void Activate_PublishesSevenRolledEvent()
        {
            SevenRolledEvent? receivedEvent = null;
            EventBus.Subscribe<SevenRolledEvent>(gameEvent => receivedEvent = gameEvent);

            _robberSystem.Activate(_activePlayer);

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(_activePlayer, receivedEvent!.Value.ActivePlayer);
        }

        [Test]
        public void Activate_PlayerWithMoreThanSevenCards_PublishesDiscardRequiredEvent()
        {
            GivePlayerCards(_victimPlayer, 8);

            DiscardRequiredEvent? receivedEvent = null;
            EventBus.Subscribe<DiscardRequiredEvent>(gameEvent => receivedEvent = gameEvent);

            _robberSystem.Activate(_activePlayer);

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(_victimPlayer, receivedEvent!.Value.Player);
            Assert.AreEqual(4, receivedEvent!.Value.Count); // floor(8 / 2)
        }

        [Test]
        public void Activate_PlayerWithExactlySevenCards_DoesNotPublishDiscardRequired()
        {
            GivePlayerCards(_victimPlayer, 7);

            bool discardEventFired = false;
            EventBus.Subscribe<DiscardRequiredEvent>(_ => discardEventFired = true);

            _robberSystem.Activate(_activePlayer);

            Assert.IsFalse(discardEventFired);
        }

        [Test]
        public void Activate_PlayerWithFewerThanSevenCards_DoesNotPublishDiscardRequired()
        {
            GivePlayerCards(_victimPlayer, 3);

            bool discardEventFired = false;
            EventBus.Subscribe<DiscardRequiredEvent>(_ => discardEventFired = true);

            _robberSystem.Activate(_activePlayer);

            Assert.IsFalse(discardEventFired);
        }

        // ── MoveRobber ─────────────────────────────────────────────────────────

        [Test]
        public void MoveRobber_ClearsMustMove()
        {
            _robberSystem.Activate(_activePlayer);
            var targetCoord = new GameCore.Board.HexCoord(1, 0);

            _robberSystem.MoveRobber(targetCoord, _activePlayer, null);

            Assert.IsFalse(_robberSystem.MustMove);
        }

        [Test]
        public void MoveRobber_UpdatesCurrentPosition()
        {
            var targetCoord = new GameCore.Board.HexCoord(1, 0);
            _robberSystem.MoveRobber(targetCoord, _activePlayer, null);
            Assert.AreEqual(targetCoord, _robberSystem.CurrentPosition);
        }

        [Test]
        public void MoveRobber_UpdatesBoardRobberPosition()
        {
            var targetCoord = new GameCore.Board.HexCoord(1, 0);
            _robberSystem.MoveRobber(targetCoord, _activePlayer, null);
            Assert.AreEqual(targetCoord, _board.RobberPosition);
        }

        [Test]
        public void MoveRobber_PublishesRobberMovedEvent()
        {
            var previousPosition = _robberSystem.CurrentPosition;
            var targetCoord = new GameCore.Board.HexCoord(1, 0);

            RobberMovedEvent? receivedEvent = null;
            EventBus.Subscribe<RobberMovedEvent>(gameEvent => receivedEvent = gameEvent);

            _robberSystem.MoveRobber(targetCoord, _activePlayer, null);

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(previousPosition, receivedEvent!.Value.From);
            Assert.AreEqual(targetCoord, receivedEvent!.Value.To);
            Assert.AreEqual(_activePlayer, receivedEvent!.Value.Mover);
        }

        [Test]
        public void MoveRobber_VictimHasResources_PublishesResourceStolenEvent()
        {
            GivePlayerCards(_victimPlayer, 3);
            var targetCoord = new GameCore.Board.HexCoord(1, 0);

            ResourceStolenEvent? receivedEvent = null;
            EventBus.Subscribe<ResourceStolenEvent>(gameEvent => receivedEvent = gameEvent);

            _robberSystem.MoveRobber(targetCoord, _activePlayer, _victimPlayer);

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(_activePlayer, receivedEvent!.Value.Thief);
            Assert.AreEqual(_victimPlayer, receivedEvent!.Value.Victim);
        }

        [Test]
        public void MoveRobber_VictimHasResources_TransfersOneResourceToThief()
        {
            GivePlayerCards(_victimPlayer, 3);
            var targetCoord = new GameCore.Board.HexCoord(1, 0);
            int victimCardsBefore = CountCards(_victimPlayer);

            _robberSystem.MoveRobber(targetCoord, _activePlayer, _victimPlayer);

            Assert.AreEqual(victimCardsBefore - 1, CountCards(_victimPlayer));
            Assert.AreEqual(1, CountCards(_activePlayer));
        }

        [Test]
        public void MoveRobber_VictimHasNoResources_DoesNotPublishResourceStolenEvent()
        {
            var targetCoord = new GameCore.Board.HexCoord(1, 0);

            bool stolenEventFired = false;
            EventBus.Subscribe<ResourceStolenEvent>(_ => stolenEventFired = true);

            _robberSystem.MoveRobber(targetCoord, _activePlayer, _victimPlayer);

            Assert.IsFalse(stolenEventFired);
        }

        [Test]
        public void MoveRobber_NullVictim_DoesNotPublishResourceStolenEvent()
        {
            var targetCoord = new GameCore.Board.HexCoord(1, 0);

            bool stolenEventFired = false;
            EventBus.Subscribe<ResourceStolenEvent>(_ => stolenEventFired = true);

            _robberSystem.MoveRobber(targetCoord, _activePlayer, null);

            Assert.IsFalse(stolenEventFired);
        }

        // ── ForceDiscard ───────────────────────────────────────────────────────

        [Test]
        public void ForceDiscard_PublishesDiscardRequiredEvent()
        {
            DiscardRequiredEvent? receivedEvent = null;
            EventBus.Subscribe<DiscardRequiredEvent>(gameEvent => receivedEvent = gameEvent);

            _robberSystem.ForceDiscard(_victimPlayer, 4);

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(_victimPlayer, receivedEvent!.Value.Player);
            Assert.AreEqual(4, receivedEvent!.Value.Count);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static void GivePlayerCards(CatanPlayer player, int count)
        {
            var bundle = new ResourceBundle().Add(CatanResources.Wood, count);
            player.Resources.TryAdd(bundle);
        }

        private static int CountCards(CatanPlayer player)
        {
            int total = 0;
            foreach (var pair in player.Resources.Current.Amounts)
                total += pair.Value;
            return total;
        }
    }
}
