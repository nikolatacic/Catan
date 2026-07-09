using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.Board;
using GameCore.Resources;

namespace Catan
{
    public class CatanBoardGenerator : BoardGenerator<CatanHexTile>
    {
        // All 19 tile positions in the standard Catan layout (axial coordinates).
        private static readonly HexCoord[] AllTileCoords =
        {
            // Ring 0
            new HexCoord(0, 0),
            // Ring 1
            new HexCoord(1, 0), new HexCoord(1, -1), new HexCoord(0, -1),
            new HexCoord(-1, 0), new HexCoord(-1, 1), new HexCoord(0, 1),
            // Ring 2
            new HexCoord(2, 0), new HexCoord(2, -1), new HexCoord(2, -2),
            new HexCoord(1, -2), new HexCoord(0, -2), new HexCoord(-1, -1),
            new HexCoord(-2, 0), new HexCoord(-2, 1), new HexCoord(-2, 2),
            new HexCoord(-1, 2), new HexCoord(0, 2), new HexCoord(1, 1)
        };

        // Standard distribution: 4 wood, 4 sheep, 4 wheat, 3 ore, 3 brick, 1 desert.
        // null = desert tile.
        private static readonly CatanResourceType?[] TileTypeDistribution =
        {
            CatanResourceType.Wood,  CatanResourceType.Wood,  CatanResourceType.Wood,  CatanResourceType.Wood,
            CatanResourceType.Sheep, CatanResourceType.Sheep, CatanResourceType.Sheep, CatanResourceType.Sheep,
            CatanResourceType.Wheat, CatanResourceType.Wheat, CatanResourceType.Wheat, CatanResourceType.Wheat,
            CatanResourceType.Ore,   CatanResourceType.Ore,   CatanResourceType.Ore,
            CatanResourceType.Brick, CatanResourceType.Brick, CatanResourceType.Brick,
            null
        };

        // Dice numbers for the 18 non-desert tiles (no 7).
        private static readonly int[] DiceNumberDistribution =
        {
            2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12
        };

        // 9 fixed border edge positions: (ring-2 tile coord, outward direction index).
        // Each becomes one port; its two access vertices come from the edge's AdjacentVertices.
        // Clockwise from upper-left. Access territory per port:
        //   (-2,2)   dir4 : both vertices solo on (-2,2)
        //   (-1,2)   dir5 : vertices touch (-1,2) and (0,2)
        //   ( 1,1)   dir5 : vertices touch (0,2) and (1,1)
        //   ( 2,0)   dir0 : both vertices solo on (2,0)
        //   ( 2,-1)  dir1 : vertices touch (2,-1) and (2,-2)
        //   ( 1,-2)  dir1 : vertices touch (1,-2) and (2,-2)
        //   ( 0,-2)  dir2 : both vertices solo on (0,-2)
        //   (-1,-1)  dir3 : vertex touches (-2,0) and (-1,-1), other solo on (-1,-1)
        //   (-2, 1)  dir3 : vertices touch (-2,0) and (-2,1)
        private static readonly (HexCoord TileCoord, int DirectionIndex)[] PortEdgePositions =
        {
            (new HexCoord(-2,  2), 4),
            (new HexCoord(-1,  2), 5),
            (new HexCoord(1,   1), 5),
            (new HexCoord(2,   0), 0),
            (new HexCoord(2,  -1), 1),
            (new HexCoord(1,  -2), 1),
            (new HexCoord(0,  -2), 2),
            (new HexCoord(-1, -1), 3),
            (new HexCoord(-2,  1), 3),
        };

        // 5 specific (2:1) ports + 4 generic (3:1) ports.
        // null = generic 3:1. Order is shuffled at generation time.
        private static readonly CatanResourceType?[] PortTypeDistribution =
        {
            CatanResourceType.Wood,
            CatanResourceType.Brick,
            CatanResourceType.Sheep,
            CatanResourceType.Wheat,
            CatanResourceType.Ore,
            null, null, null, null
        };

        private const int MaxNumberAssignmentAttempts = 200;

        private readonly System.Random _random;

        public CatanBoardGenerator(System.Random random = null)
        {
            _random = random ?? new System.Random();
        }

        // BoardGenerator<T> contract — returns the grid only.
        public override HexGrid<CatanHexTile> Generate() => GenerateBoard().Grid;

        // Full generation: returns a ready-to-play CatanBoard with topology built,
        // tiles wired, ports placed, and the robber on the desert.
        public CatanBoard GenerateBoard()
        {
            var shuffledTileTypes = TileTypeDistribution.ToList();
            Shuffle(shuffledTileTypes);

            var tileTypeByCoord = new Dictionary<HexCoord, CatanResourceType?>();
            for (int tileIndex = 0; tileIndex < AllTileCoords.Length; tileIndex++)
                tileTypeByCoord[AllTileCoords[tileIndex]] = shuffledTileTypes[tileIndex];

            var assignedDiceNumbers = AssignDiceNumbersWithConstraint(tileTypeByCoord);

            var grid = BuildGrid(tileTypeByCoord, assignedDiceNumbers, out var desertCoord);
            grid.BuildTopology();

            var portSystem = BuildPortSystem(grid);
            var board = new CatanBoard(grid, portSystem);
            board.MoveRobber(desertCoord);

            return board;
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private HexGrid<CatanHexTile> BuildGrid(
            Dictionary<HexCoord, CatanResourceType?> tileTypeByCoord,
            Dictionary<HexCoord, int> diceNumberByCoord,
            out HexCoord desertCoord)
        {
            var grid = new HexGrid<CatanHexTile>();
            desertCoord = new HexCoord(0, 0);

            foreach (var tileCoord in AllTileCoords)
            {
                var resourceType = tileTypeByCoord[tileCoord];
                IResource resource = resourceType.HasValue ? CatanResources.Get(resourceType.Value) : null;
                int diceNumber = diceNumberByCoord.TryGetValue(tileCoord, out var number) ? number : 0;

                var tile = new CatanHexTile(tileCoord, resourceType, resource, diceNumber);
                grid.SetTile(tileCoord, tile);

                if (!resourceType.HasValue) desertCoord = tileCoord;
            }

            return grid;
        }

        private PortSystem BuildPortSystem(HexGrid<CatanHexTile> grid)
        {
            var portSystem = new PortSystem();

            var shuffledPortTypes = PortTypeDistribution.ToList();
            Shuffle(shuffledPortTypes);

            for (int portIndex = 0; portIndex < PortEdgePositions.Length; portIndex++)
            {
                var (tileCoord, directionIndex) = PortEdgePositions[portIndex];
                var portResourceType = shuffledPortTypes[portIndex];

                var tileEdges = grid.GetEdges(tileCoord);
                var portEdge = tileEdges[directionIndex];
                var accessVertices = portEdge.AdjacentVertices;

                int tradeRatio = portResourceType.HasValue ? 2 : 3;
                portSystem.Ports.Add(new Port(portResourceType, tradeRatio, portEdge, accessVertices));
            }

            return portSystem;
        }

        // Assigns dice numbers to non-desert tiles, ensuring no two adjacent tiles
        // both have the high-probability numbers 6 or 8. Retries up to the limit.
        private Dictionary<HexCoord, int> AssignDiceNumbersWithConstraint(
            Dictionary<HexCoord, CatanResourceType?> tileTypeByCoord)
        {
            var nonDesertCoords = AllTileCoords
                .Where(coord => tileTypeByCoord[coord].HasValue)
                .ToList();

            for (int attempt = 0; attempt < MaxNumberAssignmentAttempts; attempt++)
            {
                var shuffledNumbers = DiceNumberDistribution.ToList();
                Shuffle(shuffledNumbers);

                var diceNumberByCoord = new Dictionary<HexCoord, int>();
                for (int coordIndex = 0; coordIndex < nonDesertCoords.Count; coordIndex++)
                    diceNumberByCoord[nonDesertCoords[coordIndex]] = shuffledNumbers[coordIndex];

                if (!HasAdjacentSixOrEight(diceNumberByCoord))
                    return diceNumberByCoord;
            }

            // Fallback: return last attempted assignment (constraint best-effort).
            var fallbackNumbers = DiceNumberDistribution.ToList();
            Shuffle(fallbackNumbers);
            var fallbackByCoord = new Dictionary<HexCoord, int>();
            for (int coordIndex = 0; coordIndex < nonDesertCoords.Count; coordIndex++)
                fallbackByCoord[nonDesertCoords[coordIndex]] = fallbackNumbers[coordIndex];
            return fallbackByCoord;
        }

        // Returns true if any two adjacent tiles both have dice number 6 or 8.
        private static bool HasAdjacentSixOrEight(Dictionary<HexCoord, int> diceNumberByCoord)
        {
            var highProbabilityTileCoords = diceNumberByCoord
                .Where(pair => pair.Value == 6 || pair.Value == 8)
                .Select(pair => pair.Key)
                .ToList();

            foreach (var firstHighCoord in highProbabilityTileCoords)
            {
                foreach (var secondHighCoord in highProbabilityTileCoords)
                {
                    if (firstHighCoord == secondHighCoord) continue;
                    if (firstHighCoord.Distance(secondHighCoord) == 1) return true;
                }
            }

            return false;
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int currentIndex = list.Count - 1; currentIndex > 0; currentIndex--)
            {
                int swapIndex = _random.Next(currentIndex + 1);
                (list[currentIndex], list[swapIndex]) = (list[swapIndex], list[currentIndex]);
            }
        }
    }
}
