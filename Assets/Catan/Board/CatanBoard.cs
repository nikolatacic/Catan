using System.Collections.Generic;
using GameCore.Board;
using GameCore.Player;

namespace Catan
{
    public class CatanBoard
    {
        public HexGrid<CatanHexTile> Grid { get; }
        public Dictionary<HexVertex, Settlement> Settlements { get; } = new();
        public Dictionary<HexEdge, Road> Roads { get; } = new();
        public HexCoord RobberPosition { get; private set; }
        public PortSystem Ports { get; }

        public CatanBoard(HexGrid<CatanHexTile> grid, PortSystem ports)
        {
            Grid = grid;
            Ports = ports;
        }

        public void MoveRobber(HexCoord newPos) => throw new System.NotImplementedException();
        public List<CatanHexTile> GetTilesForNumber(int diceNumber) => throw new System.NotImplementedException();
        public List<IPlayer> GetPlayersOnTile(HexCoord coord) => throw new System.NotImplementedException();
    }
}
