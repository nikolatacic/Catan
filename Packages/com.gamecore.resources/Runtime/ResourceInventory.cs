using GameCore.Events;
using GameCore.Player;

namespace GameCore.Resources
{
    public class ResourceInventory : IResourceInventory
    {
        private ResourceBundle _current = new();
        private readonly IPlayer _owner;

        public ResourceInventory(IPlayer owner) => _owner = owner;

        public ResourceBundle Current => _current;

        public bool TryAdd(ResourceBundle bundle)
        {
            _current = _current + bundle;
            EventBus.Publish(new ResourceAddedEvent { Player = _owner, Added = bundle });
            return true;
        }

        public bool TryRemove(ResourceBundle bundle)
        {
            if (!_current.CanAfford(bundle))
            {
                EventBus.Publish(new ResourceInsufficientEvent { Player = _owner, Needed = bundle });
                return false;
            }
            foreach (var pair in bundle.Amounts)
                _current = _current.Remove(pair.Key, pair.Value);
            EventBus.Publish(new ResourceRemovedEvent { Player = _owner, Removed = bundle });
            return true;
        }

        public bool CanAfford(ResourceBundle cost) => _current.CanAfford(cost);
    }
}
