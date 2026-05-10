using System.Collections.Generic;
using GameCore.Events;
using GameCore.Player;
using GameCore.Resources;

namespace Catan
{
    public struct ResourceProducedEvent : IGameEvent
    {
        public CatanHexTile Tile;
        public List<(IPlayer Player, ResourceBundle Bundle)> Productions;
    }
}
