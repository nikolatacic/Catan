using GameCore.Events;

namespace GameCore.StateMachine
{
    public struct StateEnteredEvent<T> : IGameEvent { public IState<T> State; }
    public struct StateExitedEvent<T> : IGameEvent { public IState<T> State; }
    public struct TransitionFiredEvent<T> : IGameEvent { public IState<T> From; public IState<T> To; }
}
