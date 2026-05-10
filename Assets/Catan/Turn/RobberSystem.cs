using GameCore.Board;
using GameCore.Player;

namespace Catan
{
    public class RobberSystem
    {
        public HexCoord CurrentPosition { get; private set; }
        public bool MustMove { get; private set; }

        public void Activate(IPlayer activePlayer) => throw new System.NotImplementedException();
        public void MoveRobber(HexCoord dest, IPlayer thief, IPlayer victim) => throw new System.NotImplementedException();
        public void ForceDiscard(IPlayer player, int count) => throw new System.NotImplementedException();
    }
}
