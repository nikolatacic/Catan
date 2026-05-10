using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameCore.Events;
using GameCore.Player;

namespace GameCore.Score
{
    public class ScoreManager : MonoBehaviour
    {
        public List<IVictoryCondition> Conditions { get; } = new();
        public List<IPlayer> Players { get; } = new();

        private readonly Dictionary<IPlayer, int> _cachedScores = new();
        private IPlayer _currentLeader;

        public int GetScore(IPlayer player)
        {
            return Conditions.Sum(condition => condition.CalculatePoints(player));
        }

        public void RecalculateAll()
        {
            foreach (var player in Players)
            {
                int newScore = GetScore(player);
                _cachedScores.TryGetValue(player, out int previousScore);

                if (newScore != previousScore)
                {
                    _cachedScores[player] = newScore;
                    EventBus.Publish(new ScoreChangedEvent
                    {
                        Player = player,
                        OldScore = previousScore,
                        NewScore = newScore
                    });
                }
            }

            var newLeader = GetLeader();
            if (newLeader != _currentLeader)
            {
                var previousLeader = _currentLeader;
                _currentLeader = newLeader;
                EventBus.Publish(new LeaderChangedEvent { Previous = previousLeader, Current = newLeader });
            }
        }

        public IPlayer GetLeader()
        {
            if (Players.Count == 0) return null;

            IPlayer leader = Players[0];
            int highestScore = GetScore(leader);

            for (int playerIndex = 1; playerIndex < Players.Count; playerIndex++)
            {
                int playerScore = GetScore(Players[playerIndex]);
                if (playerScore > highestScore)
                {
                    highestScore = playerScore;
                    leader = Players[playerIndex];
                }
            }

            return leader;
        }

        public IPlayer CheckVictory()
        {
            foreach (var player in Players)
            {
                foreach (var condition in Conditions)
                {
                    if (condition.IsWinCondition(player))
                    {
                        EventBus.Publish(new VictoryAchievedEvent { Winner = player });
                        return player;
                    }
                }
            }
            return null;
        }
    }
}
