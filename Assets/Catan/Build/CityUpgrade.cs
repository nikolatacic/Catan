using GameCore.Board;
using GameCore.Build;
using GameCore.Player;
using GameCore.Resources;

namespace Catan
{
    // ── Why this exists ────────────────────────────────────────────────────────
    // Old flow: GameManager called Settlement.UpgradeToCity() BEFORE
    // BuildManager.TryPlace so the rule's switch could route by IsCity.
    // That was a hotseat-only hack — over the network a client could
    // pre-mutate state. CityUpgrade is a distinct IPlaceable that routes
    // by type. Settlement.UpgradeToCity() only runs after validation passes.
    // ──────────────────────────────────────────────────────────────────────────

    public class CityUpgrade : IPlaceable, IBuildLocation
    {
        public IPlayer Owner { get; }
        public HexVertex Location { get; }

        public CityUpgrade(IPlayer owner, HexVertex location)
        {
            Owner = owner;
            Location = location;
        }

        // IPlaceable
        public string PlaceableId => "city_upgrade";

        public ResourceBundle BuildCost => new ResourceBundle()
            .Add(CatanResources.Wheat, 2)
            .Add(CatanResources.Ore, 3);

        // IBuildLocation
        public string LocationId => $"vertex_{Location?.GetHashCode()}";
        public bool IsOccupied => true;
        public IPlaceable OccupiedBy => this;
    }
}
