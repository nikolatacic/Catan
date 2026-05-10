namespace GameCore.StateMachine
{
    public interface IState<TContext>
    {
        void Enter(TContext context);
        void Execute(TContext context);
        void Exit(TContext context);
    }
}
