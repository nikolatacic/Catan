using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.Player;

namespace Catan
{
    public class PortSystem
    {
        public List<Port> Ports { get; } = new();

        // Returns the best (lowest) trade ratio the player can use for the given resource.
        // 2 = specific 2:1 port, 3 = generic 3:1 port, 4 = default bank rate.
        public int GetTradeRatio(IPlayer player, CatanResourceType resource)
        {
            var catanPlayer = (CatanPlayer)player;
            int bestRatio = 4;

            foreach (var port in Ports)
            {
                bool playerCanAccessPort = catanPlayer.Settlements
                    .Any(settlement => port.AccessVertices.Contains(settlement.Location));

                if (!playerCanAccessPort) continue;

                if (port.SpecificResource == null)
                    bestRatio = Math.Min(bestRatio, port.TradeRatio);
                else if (port.SpecificResource.Value == resource)
                    bestRatio = Math.Min(bestRatio, port.TradeRatio);
            }

            return bestRatio;
        }
    }
}
