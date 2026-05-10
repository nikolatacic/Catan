using System;

namespace GameCore.StateMachine
{
    public class StateTransition<TContext>
    {
        public IState<TContext> From;
        public IState<TContext> To;
        public Func<bool> Condition;
    }
}
