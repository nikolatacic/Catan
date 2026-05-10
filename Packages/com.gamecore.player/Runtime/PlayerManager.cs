using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameCore.Events;

namespace GameCore.Player
{
    public class PlayerManager : MonoBehaviour
    {
        private readonly List<IPlayer> _players = new();
        public IReadOnlyList<IPlayer> Players => _players;

        public IPlayer GetPlayer(string playerId)
        {
            return _players.FirstOrDefault(player => player.Id == playerId);
        }

        public void AddPlayer(IPlayer player)
        {
            _players.Add(player);
            EventBus.Publish(new PlayerJoinedEvent { Player = player });
        }

        public void RemovePlayer(string playerId)
        {
            var playerToRemove = _players.FirstOrDefault(player => player.Id == playerId);
            if (playerToRemove == null) return;
            _players.Remove(playerToRemove);
            EventBus.Publish(new PlayerLeftEvent { Player = playerToRemove });
        }
    }
}
