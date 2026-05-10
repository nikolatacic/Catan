using System.Collections.Generic;
using GameCore.Board;
using GameCore.Events;
using GameCore.Resources;

namespace Catan
{
    public class CatanHexTile : IHexTile
    {
        public HexCoord Coord { get; }
        public CatanResourceType? ResourceType { get; }
        public IResource Resource { get; }
        public int DiceNumber { get; }
        public bool HasRobber { get; set; }

        // Populated by CatanBoard after HexGrid.BuildTopology() is called.
        internal List<HexVertex> AdjacentVertices { get; set; } = new();

        public CatanHexTile(HexCoord coord, CatanResourceType? resourceType, IResource resource, int diceNumber)
        {
            Coord = coord;
            ResourceType = resourceType;
            Resource = resource;
            DiceNumber = diceNumber;
        }

        // Called by CatanBoard.ProduceResourcesForNumber after dice roll.
        // Settlements is the full board dictionary; this tile filters to its own vertices.
        public void ProduceResources(Dictionary<HexVertex, Settlement> allSettlements)
        {
            if (Resource == null || HasRobber) return;

            var productionList = new List<(GameCore.Player.IPlayer Player, ResourceBundle Bundle)>();

            foreach (var adjacentVertex in AdjacentVertices)
            {
                if (!allSettlements.TryGetValue(adjacentVertex, out var settlement)) continue;

                var producedBundle = new ResourceBundle().Add(Resource, settlement.ProductionMultiplier);
                settlement.Owner.Resources.TryAdd(producedBundle);
                productionList.Add((settlement.Owner, producedBundle));
            }

            if (productionList.Count > 0)
                EventBus.Publish(new ResourceProducedEvent { Tile = this, Productions = productionList });
        }
    }
}
