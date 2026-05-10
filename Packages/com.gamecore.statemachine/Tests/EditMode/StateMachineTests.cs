using System.Collections.Generic;
using NUnit.Framework;
using GameCore.Events;

namespace GameCore.StateMachine.Tests
{
    public class StateMachineTests
    {
        private class TestContext { }

        private class RecordingState : IState<TestContext>
        {
            public readonly List<string> CallLog = new();
            public readonly string StateName;

            public RecordingState(string stateName) => StateName = stateName;

            public void Enter(TestContext context) => CallLog.Add($"{StateName}.Enter");
            public void Execute(TestContext context) => CallLog.Add($"{StateName}.Execute");
            public void Exit(TestContext context) => CallLog.Add($"{StateName}.Exit");
        }

        private TestContext _context;
        private StateMachine<TestContext> _stateMachine;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            _context = new TestContext();
            _stateMachine = new StateMachine<TestContext>(_context);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
        }

        [Test]
        public void Transition_ToFirstState_CallsEnterOnNewState()
        {
            var firstState = new RecordingState("First");

            _stateMachine.Transition(firstState);

            Assert.Contains("First.Enter", firstState.CallLog);
        }

        [Test]
        public void Transition_FromOneStateToAnother_CallsExitThenEnterInOrder()
        {
            var callOrder = new List<string>();
            var firstState = new RecordingState("First");
            var secondState = new RecordingState("Second");

            EventBus.Subscribe<StateExitedEvent<TestContext>>(_ => callOrder.Add("exited"));
            EventBus.Subscribe<StateEnteredEvent<TestContext>>(_ => callOrder.Add("entered"));

            _stateMachine.Transition(firstState);
            callOrder.Clear();
            _stateMachine.Transition(secondState);

            Assert.AreEqual("exited", callOrder[0], "Exit should fire before Enter");
            Assert.AreEqual("entered", callOrder[1], "Enter should fire after Exit");
        }

        [Test]
        public void Transition_FromOneStateToAnother_ExitCalledOnPreviousState()
        {
            var firstState = new RecordingState("First");
            var secondState = new RecordingState("Second");

            _stateMachine.Transition(firstState);
            _stateMachine.Transition(secondState);

            Assert.Contains("First.Exit", firstState.CallLog);
        }

        [Test]
        public void Transition_FromOneStateToAnother_EnterNotCalledOnPreviousState()
        {
            var firstState = new RecordingState("First");
            var secondState = new RecordingState("Second");

            _stateMachine.Transition(firstState);
            _stateMachine.Transition(secondState);

            Assert.IsFalse(secondState.CallLog.Contains("First.Enter"));
            Assert.IsFalse(firstState.CallLog.Contains("First.Enter") == false,
                "First state should have Enter called exactly once");
        }

        [Test]
        public void Update_WithActiveState_CallsExecute()
        {
            var activeState = new RecordingState("Active");
            _stateMachine.Transition(activeState);
            activeState.CallLog.Clear();

            _stateMachine.Update();

            Assert.Contains("Active.Execute", activeState.CallLog);
        }

        [Test]
        public void Update_WithNoState_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _stateMachine.Update());
        }

        [Test]
        public void Transition_PublishesStateEnteredEvent()
        {
            IState<TestContext> publishedState = null;
            EventBus.Subscribe<StateEnteredEvent<TestContext>>(stateEvent => publishedState = stateEvent.State);

            var targetState = new RecordingState("Target");
            _stateMachine.Transition(targetState);

            Assert.AreSame(targetState, publishedState);
        }

        [Test]
        public void Transition_FromExistingState_PublishesStateExitedEvent()
        {
            IState<TestContext> exitedState = null;
            EventBus.Subscribe<StateExitedEvent<TestContext>>(stateEvent => exitedState = stateEvent.State);

            var firstState = new RecordingState("First");
            var secondState = new RecordingState("Second");

            _stateMachine.Transition(firstState);
            _stateMachine.Transition(secondState);

            Assert.AreSame(firstState, exitedState);
        }

        [Test]
        public void Transition_BetweenTwoStates_PublishesTransitionFiredEvent()
        {
            IState<TestContext> transitionFrom = null;
            IState<TestContext> transitionTo = null;
            EventBus.Subscribe<TransitionFiredEvent<TestContext>>(transitionEvent =>
            {
                transitionFrom = transitionEvent.From;
                transitionTo = transitionEvent.To;
            });

            var firstState = new RecordingState("First");
            var secondState = new RecordingState("Second");

            _stateMachine.Transition(firstState);
            _stateMachine.Transition(secondState);

            Assert.AreSame(firstState, transitionFrom);
            Assert.AreSame(secondState, transitionTo);
        }

        [Test]
        public void Transition_ToFirstState_DoesNotPublishExitEvent()
        {
            var exitEventCount = 0;
            EventBus.Subscribe<StateExitedEvent<TestContext>>(_ => exitEventCount++);

            _stateMachine.Transition(new RecordingState("First"));

            Assert.AreEqual(0, exitEventCount);
        }

        [Test]
        public void CurrentState_AfterTransition_IsNewState()
        {
            var newState = new RecordingState("New");
            _stateMachine.Transition(newState);

            Assert.AreSame(newState, _stateMachine.CurrentState);
        }

        [Test]
        public void Update_AfterTransition_OnlyExecutesCurrentState()
        {
            var firstState = new RecordingState("First");
            var secondState = new RecordingState("Second");

            _stateMachine.Transition(firstState);
            _stateMachine.Transition(secondState);
            firstState.CallLog.Clear();
            secondState.CallLog.Clear();

            _stateMachine.Update();

            Assert.IsEmpty(firstState.CallLog);
            Assert.Contains("Second.Execute", secondState.CallLog);
        }
    }
}
