using NUnit.Framework;
using UnityEngine;

namespace GameCore.Events.Tests
{
    public class GameEventChannelTests
    {
        private class IntChannel : GameEventChannel<int> { }

        private IntChannel _channel;

        [SetUp]
        public void SetUp()
        {
            _channel = ScriptableObject.CreateInstance<IntChannel>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_channel);
        }

        [Test]
        public void Raise_WithRegisteredListener_ListenerReceivesValue()
        {
            var receivedValue = 0;
            _channel.Register(value => receivedValue = value);

            _channel.Raise(99);

            Assert.AreEqual(99, receivedValue);
        }

        [Test]
        public void Unregister_ThenRaise_ListenerNotCalled()
        {
            var callCount = 0;
            void Listener(int value) => callCount++;

            _channel.Register(Listener);
            _channel.Unregister(Listener);
            _channel.Raise(1);

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void Raise_WithNoListeners_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _channel.Raise(42));
        }

        [Test]
        public void Raise_MultipleListeners_AllReceiveValue()
        {
            var firstReceivedValue = 0;
            var secondReceivedValue = 0;

            _channel.Register(value => firstReceivedValue = value);
            _channel.Register(value => secondReceivedValue = value);

            _channel.Raise(55);

            Assert.AreEqual(55, firstReceivedValue);
            Assert.AreEqual(55, secondReceivedValue);
        }
    }
}
