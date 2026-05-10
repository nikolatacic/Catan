namespace GameCore.StateMachine
{
    public class StateMachine<TContext>
    {
        private TContext _context;
        public IState<TContext> CurrentState { get; private set; }

        public StateMachine(TContext context) => _context = context;

        public void Transition(IState<TContext> newState)
        {
            CurrentState?.Exit(_context);
            CurrentState = newState;
            newState?.Enter(_context);
        }

        public void Update() => CurrentState?.Execute(_context);
    }
}
