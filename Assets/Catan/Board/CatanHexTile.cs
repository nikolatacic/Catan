using System.Collections.Generic;
using GameCore.Board;

namespace Catan
{
    public class CatanHexTile : IHexTile
    {
        public HexCoord Coord { get; }
        public CatanResourceType? ResourceType { get; }
        public int DiceNumber { get; }
        public bool HasRobber { get; set; }

        public CatanHexTile(HexCoord coord, CatanResourceType? resourceType, int diceNumber)
        {
            Coord = coord;
            ResourceType = resourceType;
            DiceNumber = diceNumber;
        }

        public void ProduceResources(Dictionary<HexVertex, Settlement> settlements)
            => throw new System.NotImplementedException();
    }
}
