using System.Collections.Generic;
using UnityEngine;
using GameCore.Player;

namespace GameCore.Score
{
    public class ScoreManager : MonoBehaviour
    {
        public List<IVictoryCondition> Conditions { get; } = new();

        public int GetScore(IPlayer player) => throw new System.NotImplementedException();
        public void RecalculateAll() => throw new System.NotImplementedException();
        public IPlayer GetLeader() => throw new System.NotImplementedException();
        public IPlayer CheckVictory() => throw new System.NotImplementedException();
    }
}
