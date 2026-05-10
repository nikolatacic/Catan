using System.Collections.Generic;

namespace GameCore.Board
{
    public class HexGrid<T> where T : IHexTile
    {
        private readonly Dictionary<HexCoord, T> _tiles = new();
        public IReadOnlyDictionary<HexCoord, T> Tiles => _tiles;

        public T GetTile(HexCoord coord) => _tiles.TryGetValue(coord, out var t) ? t : default;
        public void SetTile(HexCoord coord, T tile) => _tiles[coord] = tile;

        public List<T> GetNeighbors(HexCoord coord)
        {
            var result = new List<T>(6);
            foreach (var dir in HexCoord.Directions)
            {
                var neighbor = new HexCoord(coord.Q + dir.Q, coord.R + dir.R);
                if (_tiles.TryGetValue(neighbor, out var t)) result.Add(t);
            }
            return result;
        }

        public List<HexVertex> GetVertices(HexCoord coord) => throw new System.NotImplementedException();
        public List<HexEdge> GetEdges(HexCoord coord) => throw new System.NotImplementedException();
    }
}
