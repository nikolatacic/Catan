namespace GameCore.Board
{
    public class HexVertex
    {
        public HexCoord[] AdjacentTiles;
        public HexEdge[] AdjacentEdges;

        public HexVertex(HexCoord[] adjacentTiles)
        {
            AdjacentTiles = adjacentTiles;
            AdjacentEdges = System.Array.Empty<HexEdge>();
        }
    }
}
