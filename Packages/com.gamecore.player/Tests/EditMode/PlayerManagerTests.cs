using NUnit.Framework;
using UnityEngine;
using GameCore.Events;

namespace GameCore.Player.Tests
{
    public class PlayerManagerTests
    {
        private class TestPlayer : IPlayer
        {
            public string Id { get; }
            public string DisplayName { get; }
            public Color Color => Color.white;

            public TestPlayer(string playerId, string displayName = "Test Player")
            {
                Id = playerId;
                DisplayName = displayName;
            }
        }

        private PlayerManager _playerManager;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            var gameObject = new GameObject("PlayerManager");
            _playerManager = gameObject.AddComponent<PlayerManager>();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            Object.DestroyImmediate(_playerManager.gameObject);
        }

        // ── AddPlayer ──────────────────────────────────────────────────────────

        [Test]
        public void AddPlayer_PlayerAppearsInPlayersList()
        {
            var player = new TestPlayer("player-1");
            _playerManager.AddPlayer(player);

            Assert.Contains(player, (System.Collections.ICollection)_playerManager.Players);
        }

        [Test]
        public void AddPlayer_PublishesPlayerJoinedEvent()
        {
            PlayerJoinedEvent? receivedEvent = null;
            EventBus.Subscribe<PlayerJoinedEvent>(gameEvent => receivedEvent = gameEvent);

            var player = new TestPlayer("player-1");
            _playerManager.AddPlayer(player);

            Assert.IsNotNull(receivedEvent);
            Assert.AreSame(player, receivedEvent.Value.Player);
        }

        [Test]
        public void AddPlayer_MultiplePlayersStoredInOrder()
        {
            var firstPlayer = new TestPlayer("player-1");
            var secondPlayer = new TestPlayer("player-2");
            _playerManager.AddPlayer(firstPlayer);
            _playerManager.AddPlayer(secondPlayer);

            Assert.AreEqual(2, _playerManager.Players.Count);
            Assert.AreSame(firstPlayer, _playerManager.Players[0]);
            Assert.AreSame(secondPlayer, _playerManager.Players[1]);
        }

        // ── GetPlayer ──────────────────────────────────────────────────────────

        [Test]
        public void GetPlayer_WithKnownId_ReturnsCorrectPlayer()
        {
            var player = new TestPlayer("player-abc");
            _playerManager.AddPlayer(player);

            var retrieved = _playerManager.GetPlayer("player-abc");

            Assert.AreSame(player, retrieved);
        }

        [Test]
        public void GetPlayer_WithUnknownId_ReturnsNull()
        {
            var retrieved = _playerManager.GetPlayer("nonexistent-id");
            Assert.IsNull(retrieved);
        }

        [Test]
        public void GetPlayer_WithMultiplePlayers_ReturnsCorrectOne()
        {
            var firstPlayer = new TestPlayer("player-1");
            var secondPlayer = new TestPlayer("player-2");
            _playerManager.AddPlayer(firstPlayer);
            _playerManager.AddPlayer(secondPlayer);

            Assert.AreSame(secondPlayer, _playerManager.GetPlayer("player-2"));
        }

        // ── RemovePlayer ───────────────────────────────────────────────────────

        [Test]
        public void RemovePlayer_PlayerNoLongerInList()
        {
            var player = new TestPlayer("player-1");
            _playerManager.AddPlayer(player);
            _playerManager.RemovePlayer("player-1");

            Assert.AreEqual(0, _playerManager.Players.Count);
        }

        [Test]
        public void RemovePlayer_PublishesPlayerLeftEvent()
        {
            PlayerLeftEvent? receivedEvent = null;
            EventBus.Subscribe<PlayerLeftEvent>(gameEvent => receivedEvent = gameEvent);

            var player = new TestPlayer("player-1");
            _playerManager.AddPlayer(player);
            _playerManager.RemovePlayer("player-1");

            Assert.IsNotNull(receivedEvent);
            Assert.AreSame(player, receivedEvent.Value.Player);
        }

        [Test]
        public void RemovePlayer_UnknownId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _playerManager.RemovePlayer("nonexistent-id"));
        }

        [Test]
        public void RemovePlayer_UnknownId_DoesNotPublishEvent()
        {
            var leftEventCount = 0;
            EventBus.Subscribe<PlayerLeftEvent>(_ => leftEventCount++);

            _playerManager.RemovePlayer("nonexistent-id");

            Assert.AreEqual(0, leftEventCount);
        }

        [Test]
        public void RemovePlayer_OnlyRemovesMatchingPlayer()
        {
            var firstPlayer = new TestPlayer("player-1");
            var secondPlayer = new TestPlayer("player-2");
            _playerManager.AddPlayer(firstPlayer);
            _playerManager.AddPlayer(secondPlayer);

            _playerManager.RemovePlayer("player-1");

            Assert.AreEqual(1, _playerManager.Players.Count);
            Assert.AreSame(secondPlayer, _playerManager.Players[0]);
        }
    }
}
