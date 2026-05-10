using GameCore.Board;
using GameCore.Build;
using GameCore.Player;
using GameCore.Resources;

namespace Catan
{
    public class Road : IPlaceable
    {
        public IPlayer Owner { get; }
        public HexEdge Location { get; }

        public Road(IPlayer owner, HexEdge location)
        {
            Owner = owner;
            Location = location;
        }

        public string PlaceableId => "road";
        public ResourceBundle BuildCost => throw new System.NotImplementedException();
    }
}
