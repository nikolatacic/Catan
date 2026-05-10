using GameCore.Events;
using GameCore.Player;

namespace GameCore.Build
{
    public struct BuildAttemptedEvent : IGameEvent { public IPlayer Player; public IPlaceable Piece; public IBuildLocation Location; }
    public struct BuildSucceededEvent : IGameEvent { public IPlayer Player; public IPlaceable Piece; public IBuildLocation Location; }
    public struct BuildFailedEvent : IGameEvent { public IPlayer Player; public string Reason; }
    public struct PieceRemovedEvent : IGameEvent { public IPlaceable Piece; public IBuildLocation Location; }
}
