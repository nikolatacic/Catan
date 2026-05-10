using GameCore.Events;

namespace GameCore.StateMachine
{
    public class StateMachine<TContext>
    {
        private readonly TContext _context;
        public IState<TContext> CurrentState { get; private set; }

        public StateMachine(TContext context) => _context = context;

        public void Transition(IState<TContext> newState)
        {
            var previousState = CurrentState;
            previousState?.Exit(_context);
            if (previousState != null)
                EventBus.Publish(new StateExitedEvent<TContext> { State = previousState });

            CurrentState = newState;
            newState?.Enter(_context);
            if (newState != null)
                EventBus.Publish(new StateEnteredEvent<TContext> { State = newState });

            if (previousState != null && newState != null)
                EventBus.Publish(new TransitionFiredEvent<TContext> { From = previousState, To = newState });
        }

        public void Update() => CurrentState?.Execute(_context);
    }
}
