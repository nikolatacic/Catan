using GameCore.Events;

namespace Catan.UI
{
    public struct PlacementModeChangedEvent : IGameEvent
    {
        public PlacementMode Mode;
    }
}
