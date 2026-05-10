using GameCore.Player;

namespace GameCore.Score
{
    public interface IVictoryCondition
    {
        string Name { get; }
        int CalculatePoints(IPlayer player);
        bool IsWinCondition(IPlayer player);
    }
}
