using NUnit.Framework;

namespace GameCore.Events.Tests
{
    public class EventBusTests
    {
        private struct TestEvent : IGameEvent
        {
            public int Value;
        }

        private struct AnotherTestEvent : IGameEvent
        {
            public string Message;
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
        }

        [Test]
        public void Subscribe_ThenPublish_HandlerReceivesEvent()
        {
            var receivedValue = 0;
            EventBus.Subscribe<TestEvent>(testEvent => receivedValue = testEvent.Value);

            EventBus.Publish(new TestEvent { Value = 42 });

            Assert.AreEqual(42, receivedValue);
        }

        [Test]
        public void Unsubscribe_ThenPublish_HandlerNotCalled()
        {
            var callCount = 0;
            void Handler(TestEvent testEvent) => callCount++;

            EventBus.Subscribe<TestEvent>(Handler);
            EventBus.Unsubscribe<TestEvent>(Handler);
            EventBus.Publish(new TestEvent { Value = 1 });

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void MultipleSubscribers_AllReceivePublishedEvent()
        {
            var firstReceivedValue = 0;
            var secondReceivedValue = 0;

            EventBus.Subscribe<TestEvent>(testEvent => firstReceivedValue = testEvent.Value);
            EventBus.Subscribe<TestEvent>(testEvent => secondReceivedValue = testEvent.Value);

            EventBus.Publish(new TestEvent { Value = 7 });

            Assert.AreEqual(7, firstReceivedValue);
            Assert.AreEqual(7, secondReceivedValue);
        }

        [Test]
        public void Unsubscribe_OneOfTwoSubscribers_OtherStillReceivesEvent()
        {
            var firstCallCount = 0;
            var secondCallCount = 0;
            void FirstHandler(TestEvent testEvent) => firstCallCount++;
            void SecondHandler(TestEvent testEvent) => secondCallCount++;

            EventBus.Subscribe<TestEvent>(FirstHandler);
            EventBus.Subscribe<TestEvent>(SecondHandler);
            EventBus.Unsubscribe<TestEvent>(FirstHandler);

            EventBus.Publish(new TestEvent());

            Assert.AreEqual(0, firstCallCount);
            Assert.AreEqual(1, secondCallCount);
        }

        [Test]
        public void Publish_DifferentEventTypes_DoNotCrossfire()
        {
            var testEventCallCount = 0;
            var anotherEventCallCount = 0;

            EventBus.Subscribe<TestEvent>(_ => testEventCallCount++);
            EventBus.Subscribe<AnotherTestEvent>(_ => anotherEventCallCount++);

            EventBus.Publish(new TestEvent());

            Assert.AreEqual(1, testEventCallCount);
            Assert.AreEqual(0, anotherEventCallCount);
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => EventBus.Publish(new TestEvent { Value = 99 }));
        }

        [Test]
        public void Publish_HandlerThrowsException_OtherHandlersStillCalled()
        {
            var secondHandlerCallCount = 0;

            EventBus.Subscribe<TestEvent>(_ => throw new System.Exception("handler error"));
            EventBus.Subscribe<TestEvent>(_ => secondHandlerCallCount++);

            Assert.DoesNotThrow(() => EventBus.Publish(new TestEvent()));
            Assert.AreEqual(1, secondHandlerCallCount);
        }

        [Test]
        public void Unsubscribe_HandlerNotRegistered_DoesNotThrow()
        {
            void Handler(TestEvent testEvent) { }
            Assert.DoesNotThrow(() => EventBus.Unsubscribe<TestEvent>(Handler));
        }

        [Test]
        public void Clear_RemovesAllHandlers_PublishDoesNotCallAnything()
        {
            var callCount = 0;
            EventBus.Subscribe<TestEvent>(_ => callCount++);

            EventBus.Clear();
            EventBus.Publish(new TestEvent());

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void Subscribe_SameHandlerTwice_CalledTwiceOnPublish()
        {
            var callCount = 0;
            void Handler(TestEvent testEvent) => callCount++;

            EventBus.Subscribe<TestEvent>(Handler);
            EventBus.Subscribe<TestEvent>(Handler);

            EventBus.Publish(new TestEvent());

            Assert.AreEqual(2, callCount);
        }
    }
}
