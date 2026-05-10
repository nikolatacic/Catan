using GameCore.Events;
using GameCore.Player;

namespace Catan
{
    public struct LongestRoadChangedEvent : IGameEvent { public IPlayer Previous; public IPlayer Current; }

    public class LongestRoadTracker
    {
        public IPlayer CurrentHolder { get; private set; }
        public int CurrentLength { get; private set; }

        public void Recalculate(CatanBoard board) => throw new System.NotImplementedException();
    }
}
