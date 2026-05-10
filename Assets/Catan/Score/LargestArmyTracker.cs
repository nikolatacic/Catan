using GameCore.Events;
using GameCore.Player;

namespace Catan
{
    public struct LargestArmyChangedEvent : IGameEvent { public IPlayer Previous; public IPlayer Current; }

    public class LargestArmyTracker
    {
        private const int MinimumKnightsToQualify = 3;

        public IPlayer CurrentHolder { get; private set; }
        public int CurrentCount { get; private set; }

        // Call this whenever a player plays a knight.
        // The token transfers only when a player strictly exceeds the current holder's count.
        public void Update(IPlayer player, int knightsPlayed)
        {
            if (knightsPlayed < MinimumKnightsToQualify) return;

            // Already the holder — just update count; no token transfer needed.
            if (CurrentHolder == player)
            {
                CurrentCount = knightsPlayed;
                return;
            }

            // Must strictly beat the current count to take the token.
            if (knightsPlayed <= CurrentCount) return;

            var previousHolder = CurrentHolder;

            if (previousHolder is CatanPlayer previousCatanPlayer)
                previousCatanPlayer.HasLargestArmy = false;

            CurrentHolder = player;
            CurrentCount = knightsPlayed;

            if (player is CatanPlayer newCatanPlayer)
                newCatanPlayer.HasLargestArmy = true;

            EventBus.Publish(new LargestArmyChangedEvent { Previous = previousHolder, Current = player });
        }
    }
}
