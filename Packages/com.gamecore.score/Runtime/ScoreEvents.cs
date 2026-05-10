using GameCore.Events;
using GameCore.Player;

namespace GameCore.Score
{
    public struct ScoreChangedEvent : IGameEvent { public IPlayer Player; public int OldScore; public int NewScore; }
    public struct LeaderChangedEvent : IGameEvent { public IPlayer Previous; public IPlayer Current; }
    public struct VictoryAchievedEvent : IGameEvent { public IPlayer Winner; }
}
