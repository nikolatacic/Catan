using NUnit.Framework;
using GameCore.Events;
using GameCore.Resources;
using UnityEngine;

namespace Catan.Tests
{
    public class LargestArmyTrackerTests
    {
        private class TestResource : IResource
        {
            public string ResourceId { get; }
            public string DisplayName { get; }
            public TestResource(string resourceId) { ResourceId = resourceId; DisplayName = resourceId; }
        }

        private LargestArmyTracker _tracker;
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

            _tracker = new LargestArmyTracker();
            _playerRed = new CatanPlayer("red", "Red", Color.red);
            _playerBlue = new CatanPlayer("blue", "Blue", Color.blue);
        }

        [TearDown]
        public void TearDown() => EventBus.Clear();

        // ── No effect below threshold ──────────────────────────────────────────

        [Test]
        public void Update_LessThanThreeKnights_NoTokenAwarded()
        {
            _tracker.Update(_playerRed, 2);
            Assert.IsNull(_tracker.CurrentHolder);
        }

        [Test]
        public void Update_LessThanThreeKnights_HasLargestArmyRemainesFalse()
        {
            _tracker.Update(_playerRed, 2);
            Assert.IsFalse(_playerRed.HasLargestArmy);
        }

        // ── First player to reach three gets the token ─────────────────────────

        [Test]
        public void Update_ThreeKnights_AwardsToken()
        {
            _tracker.Update(_playerRed, 3);
            Assert.AreEqual(_playerRed, _tracker.CurrentHolder);
        }

        [Test]
        public void Update_ThreeKnights_SetsHasLargestArmyTrue()
        {
            _tracker.Update(_playerRed, 3);
            Assert.IsTrue(_playerRed.HasLargestArmy);
        }

        [Test]
        public void Update_ThreeKnights_PublishesLargestArmyChangedEvent()
        {
            LargestArmyChangedEvent? receivedEvent = null;
            EventBus.Subscribe<LargestArmyChangedEvent>(gameEvent => receivedEvent = gameEvent);

            _tracker.Update(_playerRed, 3);

            Assert.IsNotNull(receivedEvent);
            Assert.IsNull(receivedEvent!.Value.Previous);
            Assert.AreEqual(_playerRed, receivedEvent!.Value.Current);
        }

        // ── Current holder updates count without re-publishing ─────────────────

        [Test]
        public void Update_CurrentHolderIncreasesCount_NoEventPublished()
        {
            _tracker.Update(_playerRed, 3);

            bool eventPublished = false;
            EventBus.Subscribe<LargestArmyChangedEvent>(_ => eventPublished = true);

            _tracker.Update(_playerRed, 4);

            Assert.IsFalse(eventPublished);
        }

        // ── Must strictly beat current count to take the token ─────────────────

        [Test]
        public void Update_OtherPlayerMatchesCount_TokenDoesNotTransfer()
        {
            _tracker.Update(_playerRed, 3);
            _tracker.Update(_playerBlue, 3); // ties don't transfer
            Assert.AreEqual(_playerRed, _tracker.CurrentHolder);
        }

        [Test]
        public void Update_OtherPlayerExceedsCount_TokenTransfers()
        {
            _tracker.Update(_playerRed, 3);
            _tracker.Update(_playerBlue, 4);
            Assert.AreEqual(_playerBlue, _tracker.CurrentHolder);
        }

        [Test]
        public void Update_TokenTransfer_PreviousHolderLosesArmyFlag()
        {
            _tracker.Update(_playerRed, 3);
            _tracker.Update(_playerBlue, 4);
            Assert.IsFalse(_playerRed.HasLargestArmy);
            Assert.IsTrue(_playerBlue.HasLargestArmy);
        }

        [Test]
        public void Update_TokenTransfer_PublishesEventWithCorrectPlayers()
        {
            _tracker.Update(_playerRed, 3);

            LargestArmyChangedEvent? receivedEvent = null;
            EventBus.Subscribe<LargestArmyChangedEvent>(gameEvent => receivedEvent = gameEvent);

            _tracker.Update(_playerBlue, 4);

            Assert.IsNotNull(receivedEvent);
            Assert.AreEqual(_playerRed, receivedEvent!.Value.Previous);
            Assert.AreEqual(_playerBlue, receivedEvent!.Value.Current);
        }
    }
}
