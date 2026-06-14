using GameCore.Board;
using GameCore.Build;
using GameCore.Player;
using GameCore.Resources;

namespace Catan
{
    public class Road : IPlaceable, IBuildLocation
    {
        public IPlayer Owner { get; }
        public HexEdge Location { get; }

        public Road(IPlayer owner, HexEdge location)
        {
            Owner = owner;
            Location = location;
        }

        // IPlaceable
        public string PlaceableId => "road";

        public ResourceBundle BuildCost => new ResourceBundle()
            .Add(CatanResources.Wood, 1)
            .Add(CatanResources.Brick, 1);

        // IBuildLocation
        public string LocationId => $"edge_{Location?.GetHashCode()}";
        public bool IsOccupied => true; // a Road object's existence means the edge is occupied
        public IPlaceable OccupiedBy => this;
    }
}
