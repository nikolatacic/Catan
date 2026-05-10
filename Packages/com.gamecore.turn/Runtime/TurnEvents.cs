using GameCore.Events;

namespace GameCore.Turn
{
    public struct TurnStartedEvent : IGameEvent { public ITurnActor Actor; public int TurnNumber; }
    public struct TurnEndedEvent : IGameEvent { public ITurnActor Actor; }
    public struct PhaseChangedEvent : IGameEvent { public TurnPhase From; public TurnPhase To; }
    public struct ActorSkippedEvent : IGameEvent { public ITurnActor Actor; public string Reason; }
}
