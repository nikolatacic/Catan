using GameCore.Board;
using GameCore.Build;
using GameCore.Player;
using GameCore.Resources;

namespace Catan
{
    public class Settlement : IPlaceable, IBuildLocation
    {
        public IPlayer Owner { get; }
        public HexVertex Location { get; }
        public bool IsCity { get; private set; }
        public int ProductionMultiplier => IsCity ? 2 : 1;

        public Settlement(IPlayer owner, HexVertex location)
        {
            Owner = owner;
            Location = location;
        }

        public void UpgradeToCity() => IsCity = true;

        // IPlaceable
        public string PlaceableId => IsCity ? "city" : "settlement";

        public ResourceBundle BuildCost => IsCity
            ? new ResourceBundle()
                .Add(CatanResources.Wheat, 2)
                .Add(CatanResources.Ore, 3)
            : new ResourceBundle()
                .Add(CatanResources.Wood, 1)
                .Add(CatanResources.Brick, 1)
                .Add(CatanResources.Sheep, 1)
                .Add(CatanResources.Wheat, 1);

        // IBuildLocation
        public string LocationId => $"vertex_{Location?.GetHashCode()}";
        public bool IsOccupied => IsCity;
        public IPlaceable OccupiedBy => IsCity ? this : null;
    }
}
