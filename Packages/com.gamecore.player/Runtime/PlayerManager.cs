using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Player
{
    public class PlayerManager : MonoBehaviour
    {
        private readonly List<IPlayer> _players = new();
        public IReadOnlyList<IPlayer> Players => _players;

        public IPlayer GetPlayer(string id) => throw new System.NotImplementedException();
        public void AddPlayer(IPlayer player) => throw new System.NotImplementedException();
        public void RemovePlayer(string id) => throw new System.NotImplementedException();
    }
}
