using System.Collections.Generic;
using GameCore.Board;
using GameCore.Events;
using GameCore.Player;
using GameCore.Resources;

namespace Catan
{
    public class RobberSystem
    {
        private const int HandSizeBeforeDiscard = 7;

        private readonly System.Random _random;
        private CatanBoard _board;
        private IReadOnlyList<IPlayer> _allPlayers;

        public HexCoord CurrentPosition { get; private set; }
        public bool MustMove { get; private set; }

        public RobberSystem(System.Random random = null)
        {
            _random = random ?? new System.Random();
        }

        public void Initialize(CatanBoard board, IReadOnlyList<IPlayer> allPlayers)
        {
            _board = board;
            _allPlayers = allPlayers;
            CurrentPosition = board.RobberPosition;
        }

        // Called when a 7 is rolled or a knight is played.
        // Triggers discard requirements for all players with more than 7 cards,
        // then flags that the robber must be moved.
        public void Activate(IPlayer activePlayer)
        {
            MustMove = true;
            EventBus.Publish(new SevenRolledEvent { ActivePlayer = activePlayer });

            foreach (var player in _allPlayers)
            {
                int totalCards = CountCards(player);
                if (totalCards > HandSizeBeforeDiscard)
                    ForceDiscard(player, totalCards / 2);
            }
        }

        // Moves the robber to a new tile, optionally stealing one resource from the victim.
        public void MoveRobber(HexCoord destination, IPlayer thief, IPlayer victim)
        {
            var previousPosition = CurrentPosition;
            MustMove = false;
            CurrentPosition = destination;
            _board.MoveRobber(destination);

            EventBus.Publish(new RobberMovedEvent { From = previousPosition, To = destination, Mover = thief });

            if (victim == null) return;

            var stolenResource = PickRandomResource(victim);
            if (stolenResource == null) return;

            var stolenBundle = new ResourceBundle().Add(stolenResource, 1);
            var victimInventory = GetInventory(victim);
            var thiefInventory = GetInventory(thief);

            if (victimInventory == null || thiefInventory == null) return;

            victimInventory.TryRemove(stolenBundle);
            thiefInventory.TryAdd(stolenBundle);

            EventBus.Publish(new ResourceStolenEvent { Thief = thief, Victim = victim, Stolen = stolenResource });
        }

        // Publishes a discard requirement for the player. The actual removal happens
        // when the player (or UI) calls ResourceInventory.TryRemove with their chosen cards.
        public void ForceDiscard(IPlayer player, int count)
        {
            EventBus.Publish(new DiscardRequiredEvent { Player = player, Count = count });
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private static int CountCards(IPlayer player)
        {
            var inventory = GetInventory(player);
            if (inventory == null) return 0;

            int total = 0;
            foreach (var pair in inventory.Current.Amounts)
                total += pair.Value;
            return total;
        }

        private IResource PickRandomResource(IPlayer player)
        {
            var inventory = GetInventory(player);
            if (inventory == null) return null;

            var availableResources = new List<IResource>();
            foreach (var pair in inventory.Current.Amounts)
                for (int cardIndex = 0; cardIndex < pair.Value; cardIndex++)
                    availableResources.Add(pair.Key);

            if (availableResources.Count == 0) return null;
            return availableResources[_random.Next(availableResources.Count)];
        }

        private static ResourceInventory GetInventory(IPlayer player)
        {
            return player is CatanPlayer catanPlayer ? catanPlayer.Resources : null;
        }
    }
}
