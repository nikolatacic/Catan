using GameCore.Events;
using GameCore.Player;

namespace Catan
{
    public struct LargestArmyChangedEvent : IGameEvent { public IPlayer Previous; public IPlayer Current; }

    public class LargestArmyTracker
    {
        public IPlayer CurrentHolder { get; private set; }
        public int CurrentCount { get; private set; }

        public void Update(IPlayer player, int knightsPlayed) => throw new System.NotImplementedException();
    }
}
