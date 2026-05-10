using System.Collections.Generic;
using System.Linq;
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

            // Wire adjacent vertices onto each tile so ProduceResources can filter.
            foreach (var tile in grid.Tiles.Values)
                tile.AdjacentVertices = grid.GetVertices(tile.Coord);
        }

        public void MoveRobber(HexCoord newPosition)
        {
            var previousTile = Grid.GetTile(RobberPosition);
            if (previousTile != null) previousTile.HasRobber = false;

            var newTile = Grid.GetTile(newPosition);
            if (newTile != null) newTile.HasRobber = true;

            RobberPosition = newPosition;
        }

        // Returns tiles with the given dice number that are not blocked by the robber.
        public List<CatanHexTile> GetTilesForNumber(int diceNumber)
        {
            return Grid.Tiles.Values
                .Where(tile => tile.DiceNumber == diceNumber && !tile.HasRobber)
                .ToList();
        }

        // Returns distinct players who have a settlement on any vertex of the given tile.
        public List<IPlayer> GetPlayersOnTile(HexCoord coord)
        {
            var tileVertices = Grid.GetVertices(coord);
            return tileVertices
                .Where(vertex => Settlements.ContainsKey(vertex))
                .Select(vertex => Settlements[vertex].Owner)
                .Distinct()
                .ToList();
        }

        // Convenience method called by CatanTurnManager after a dice roll (non-7).
        public void ProduceResourcesForNumber(int diceNumber)
        {
            foreach (var tile in GetTilesForNumber(diceNumber))
                tile.ProduceResources(Settlements);
        }
    }
}
