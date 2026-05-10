using System.Collections.Generic;
using GameCore.Player;

namespace Catan
{
    public class PortSystem
    {
        public List<Port> Ports { get; } = new();

        public int GetTradeRatio(IPlayer player, CatanResourceType resource)
            => throw new System.NotImplementedException();
    }
}
