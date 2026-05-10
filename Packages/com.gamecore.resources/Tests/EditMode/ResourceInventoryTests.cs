using NUnit.Framework;
using UnityEngine;
using GameCore.Events;
using GameCore.Player;

namespace GameCore.Resources.Tests
{
    public class ResourceInventoryTests
    {
        private class TestResource : IResource
        {
            public string ResourceId { get; }
            public string DisplayName { get; }
            public TestResource(string resourceId) { ResourceId = resourceId; DisplayName = resourceId; }
        }

        private class TestPlayer : IPlayer
        {
            public string Id => "test-player";
            public string DisplayName => "Test Player";
            public UnityEngine.Color Color => UnityEngine.Color.white;
        }

        private readonly TestResource _wood = new("wood");
        private readonly TestResource _brick = new("brick");

        private TestPlayer _player;
        private ResourceInventory _inventory;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            _player = new TestPlayer();
            _inventory = new ResourceInventory(_player);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
        }

        // ── TryAdd ─────────────────────────────────────────────────────────────

        [Test]
        public void TryAdd_AlwaysReturnsTrue()
        {
            var bundle = new ResourceBundle().Add(_wood, 3);
            Assert.IsTrue(_inventory.TryAdd(bundle));
        }

        [Test]
        public void TryAdd_UpdatesCurrentBundle()
        {
            _inventory.TryAdd(new ResourceBundle().Add(_wood, 2).Add(_brick, 1));

            Assert.AreEqual(2, _inventory.Current.Get(_wood));
            Assert.AreEqual(1, _inventory.Current.Get(_brick));
        }

        [Test]
        public void TryAdd_PublishesResourceAddedEvent()
        {
            ResourceAddedEvent? receivedEvent = null;
            EventBus.Subscribe<ResourceAddedEvent>(gameEvent => receivedEvent = gameEvent);

            var addedBundle = new ResourceBundle().Add(_wood, 3);
            _inventory.TryAdd(addedBundle);

            Assert.IsNotNull(receivedEvent);
            Assert.AreSame(_player, receivedEvent.Value.Player);
            Assert.AreEqual(3, receivedEvent.Value.Added.Get(_wood));
        }

        [Test]
        public void TryAdd_AccumulatesAcrossMultipleCalls()
        {
            _inventory.TryAdd(new ResourceBundle().Add(_wood, 2));
            _inventory.TryAdd(new ResourceBundle().Add(_wood, 3));

            Assert.AreEqual(5, _inventory.Current.Get(_wood));
        }

        // ── TryRemove ──────────────────────────────────────────────────────────

        [Test]
        public void TryRemove_SufficientResources_ReturnsTrue()
        {
            _inventory.TryAdd(new ResourceBundle().Add(_wood, 5));
            Assert.IsTrue(_inventory.TryRemove(new ResourceBundle().Add(_wood, 3)));
        }

        [Test]
        public void TryRemove_SufficientResources_UpdatesCurrentBundle()
        {
            _inventory.TryAdd(new ResourceBundle().Add(_wood, 5));
            _inventory.TryRemove(new ResourceBundle().Add(_wood, 3));

            Assert.AreEqual(2, _inventory.Current.Get(_wood));
        }

        [Test]
        public void TryRemove_SufficientResources_PublishesResourceRemovedEvent()
        {
            ResourceRemovedEvent? receivedEvent = null;
            EventBus.Subscribe<ResourceRemovedEvent>(gameEvent => receivedEvent = gameEvent);

            _inventory.TryAdd(new ResourceBundle().Add(_wood, 5));
            var removedBundle = new ResourceBundle().Add(_wood, 2);
            _inventory.TryRemove(removedBundle);

            Assert.IsNotNull(receivedEvent);
            Assert.AreSame(_player, receivedEvent.Value.Player);
            Assert.AreEqual(2, receivedEvent.Value.Removed.Get(_wood));
        }

        [Test]
        public void TryRemove_InsufficientResources_ReturnsFalse()
        {
            _inventory.TryAdd(new ResourceBundle().Add(_wood, 1));
            Assert.IsFalse(_inventory.TryRemove(new ResourceBundle().Add(_wood, 5)));
        }

        [Test]
        public void TryRemove_InsufficientResources_DoesNotChangeCurrentBundle()
        {
            _inventory.TryAdd(new ResourceBundle().Add(_wood, 1));
            _inventory.TryRemove(new ResourceBundle().Add(_wood, 5));

            Assert.AreEqual(1, _inventory.Current.Get(_wood));
        }

        [Test]
        public void TryRemove_InsufficientResources_PublishesResourceInsufficientEvent()
        {
            ResourceInsufficientEvent? receivedEvent = null;
            EventBus.Subscribe<ResourceInsufficientEvent>(gameEvent => receivedEvent = gameEvent);

            var neededBundle = new ResourceBundle().Add(_wood, 5);
            _inventory.TryRemove(neededBundle);

            Assert.IsNotNull(receivedEvent);
            Assert.AreSame(_player, receivedEvent.Value.Player);
            Assert.AreEqual(5, receivedEvent.Value.Needed.Get(_wood));
        }

        [Test]
        public void TryRemove_InsufficientResources_DoesNotPublishRemovedEvent()
        {
            var removedEventCount = 0;
            EventBus.Subscribe<ResourceRemovedEvent>(_ => removedEventCount++);

            _inventory.TryRemove(new ResourceBundle().Add(_wood, 1));

            Assert.AreEqual(0, removedEventCount);
        }

        // ── CanAfford ──────────────────────────────────────────────────────────

        [Test]
        public void CanAfford_DelegatesToCurrentBundle()
        {
            _inventory.TryAdd(new ResourceBundle().Add(_wood, 3));

            Assert.IsTrue(_inventory.CanAfford(new ResourceBundle().Add(_wood, 3)));
            Assert.IsFalse(_inventory.CanAfford(new ResourceBundle().Add(_wood, 4)));
        }
    }
}
