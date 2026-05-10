using GameCore.Events;

namespace GameCore.Player
{
    public struct PlayerJoinedEvent : IGameEvent { public IPlayer Player; }
    public struct PlayerLeftEvent : IGameEvent { public IPlayer Player; }
    public struct ActivePlayerChangedEvent : IGameEvent { public IPlayer Prev; public IPlayer Next; }
}
