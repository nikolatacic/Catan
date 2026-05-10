using GameCore.Board;

namespace Catan
{
    public class Port
    {
        public CatanResourceType? SpecificResource;
        public int TradeRatio;
        public HexEdge Location;
        public HexVertex[] AccessVertices;

        public Port(CatanResourceType? resource, int ratio, HexEdge location, HexVertex[] accessVertices)
        {
            SpecificResource = resource;
            TradeRatio = ratio;
            Location = location;
            AccessVertices = accessVertices;
        }
    }
}
