using System;
using GameCore.Resources;

namespace Catan
{
    // Initialized once at game startup (or in tests). Provides the canonical IResource
    // instance for each CatanResourceType so build costs and tile production can
    // reference the same object that lives in ResourceInventory keys.
    public static class CatanResources
    {
        public static IResource Wood  { get; private set; }
        public static IResource Brick { get; private set; }
        public static IResource Sheep { get; private set; }
        public static IResource Wheat { get; private set; }
        public static IResource Ore   { get; private set; }

        public static void Initialize(
            IResource wood, IResource brick, IResource sheep, IResource wheat, IResource ore)
        {
            Wood  = wood;
            Brick = brick;
            Sheep = sheep;
            Wheat = wheat;
            Ore   = ore;
        }

        public static IResource[] All => new[] { Wood, Brick, Sheep, Wheat, Ore };

        public static IResource Get(CatanResourceType resourceType) => resourceType switch
        {
            CatanResourceType.Wood  => Wood,
            CatanResourceType.Brick => Brick,
            CatanResourceType.Sheep => Sheep,
            CatanResourceType.Wheat => Wheat,
            CatanResourceType.Ore   => Ore,
            _ => throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, null)
        };
    }
}
