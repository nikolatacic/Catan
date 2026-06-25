using System.Linq;
using GameCore.Player;
using GameCore.Score;

namespace Catan
{
    public class CatanVictoryCondition : IVictoryCondition
    {
        private CatanBoard _board;
        public int TargetPoints { get; set; }

        public CatanVictoryCondition(CatanBoard board, int targetPoints = 10)
        {
            _board = board;
            TargetPoints = targetPoints;
        }

        public string Name => "Catan Victory";

        public int CalculatePoints(IPlayer player)
        {
            var cp = (CatanPlayer)player;
            return cp.Settlements.Count(s => !s.IsCity)
                 + cp.Settlements.Count(s => s.IsCity) * 2
                 + cp.DevelopmentCards.Cards.OfType<VictoryPointCard>().Sum(v => v.Points)
                 + (cp.HasLargestArmy ? 2 : 0)
                 + (cp.HasLongestRoad ? 2 : 0);
        }

        public bool IsWinCondition(IPlayer player) => CalculatePoints(player) >= TargetPoints;
    }
}
