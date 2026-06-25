using GameCore.Board;
using GameCore.Events;
using GameCore.Player;
using GameCore.Resources;

namespace Catan
{
    public struct DiceRolledEvent : IGameEvent { public int D1; public int D2; public int Total; }
    public struct SevenRolledEvent : IGameEvent { public IPlayer ActivePlayer; }
    public struct RobberMovedEvent : IGameEvent { public HexCoord From; public HexCoord To; public IPlayer Mover; }
    public struct ResourceStolenEvent : IGameEvent { public IPlayer Thief; public IPlayer Victim; public IResource Stolen; }
    public struct DiscardRequiredEvent : IGameEvent { public IPlayer Player; public int Count; }
    public struct KnightPlayedEvent : IGameEvent { public IPlayer Player; }
    public struct DevCardPurchasedEvent : IGameEvent { public IPlayer Player; }
    public struct CatanPhaseChangedEvent : IGameEvent { public CatanTurnPhase From; public CatanTurnPhase To; }

    // Published after GameManager finishes mutating game state at the end of an
    // action. UI views that need to see the FINAL state (placement mode,
    // resources, score, IsGameOver) should subscribe to this rather than
    // BuildSucceededEvent, which fires mid-action before state has settled.
    public struct GameStateChangedEvent : IGameEvent { }
}
