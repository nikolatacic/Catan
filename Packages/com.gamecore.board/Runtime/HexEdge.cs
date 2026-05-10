namespace GameCore.Board
{
    public class HexEdge
    {
        public HexCoord[] AdjacentTiles;
        public HexVertex[] AdjacentVertices;

        public HexEdge(HexCoord[] adjacentTiles, HexVertex[] adjacentVertices)
        {
            AdjacentTiles = adjacentTiles;
            AdjacentVertices = adjacentVertices;
        }
    }
}
