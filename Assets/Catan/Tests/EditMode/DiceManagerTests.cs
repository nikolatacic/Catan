using NUnit.Framework;
using GameCore.Events;
using GameCore.Resources;

namespace Catan.Tests
{
    public class DiceManagerTests
    {
        private class TestResource : IResource
        {
            public string ResourceId { get; }
            public string DisplayName { get; }
            public TestResource(string resourceId) { ResourceId = resourceId; DisplayName = resourceId; }
        }

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
        }

        [TearDown]
        public void TearDown() => EventBus.Clear();

        [Test]
        public void Roll_ReturnsValueBetweenTwoAndTwelve()
        {
            var diceManager = new DiceManager(CreateFixedRoller(3, 4));
            int result = diceManager.Roll();
            Assert.AreEqual(7, result);
        }

        [Test]
        public void Roll_ReturnsMinimumOfTwo()
        {
            var diceManager = new DiceManager(CreateFixedRoller(1, 1));
            int result = diceManager.Roll();
            Assert.AreEqual(2, result);
        }

        [Test]
        public void Roll_ReturnsMaximumOfTwelve()
        {
            var diceManager = new DiceManager(CreateFixedRoller(6, 6));
            int result = diceManager.Roll();
            Assert.AreEqual(12, result);
        }

        [Test]
        public void Roll_StoresLastRoll()
        {
            var diceManager = new DiceManager(CreateFixedRoller(2, 5));
            diceManager.Roll();
            Assert.AreEqual((2, 5), diceManager.LastRoll);
        }

        [Test]
        public void Roll_PublishesDiceRolledEvent()
        {
            var diceManager = new DiceManager(CreateFixedRoller(3, 4));

            DiceRolledEvent? receivedEvent = null;
            EventBus.Subscribe<DiceRolledEvent>(gameEvent => receivedEvent = gameEvent);

            diceManager.Roll();

            Assert.IsNotNull(receivedEvent);
        }

        [Test]
        public void Roll_PublishedEvent_ContainsCorrectValues()
        {
            var diceManager = new DiceManager(CreateFixedRoller(3, 4));

            DiceRolledEvent? receivedEvent = null;
            EventBus.Subscribe<DiceRolledEvent>(gameEvent => receivedEvent = gameEvent);

            diceManager.Roll();

            Assert.AreEqual(3, receivedEvent!.Value.D1);
            Assert.AreEqual(4, receivedEvent!.Value.D2);
            Assert.AreEqual(7, receivedEvent!.Value.Total);
        }

        [Test]
        public void Roll_LastRollUpdatesOnEachRoll()
        {
            int rollCount = 0;
            int[] dieFaces = { 2, 3, 5, 6 };

            var diceManager = new DiceManager((min, max) => dieFaces[rollCount++]);

            diceManager.Roll();
            Assert.AreEqual((2, 3), diceManager.LastRoll);

            diceManager.Roll();
            Assert.AreEqual((5, 6), diceManager.LastRoll);
        }

        // ── Helper ─────────────────────────────────────────────────────────────

        private static System.Func<int, int, int> CreateFixedRoller(int firstDie, int secondDie)
        {
            int rollIndex = 0;
            int[] results = { firstDie, secondDie };
            return (min, max) => results[rollIndex++];
        }
    }
}
