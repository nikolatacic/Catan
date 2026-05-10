using GameCore.Player;

namespace GameCore.Cards
{
    public interface IGameContext
    {
        IPlayer ActivePlayer { get; }
    }
}
