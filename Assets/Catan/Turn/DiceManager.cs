using System;
using GameCore.Events;

namespace Catan
{
    public class DiceManager
    {
        private readonly Func<int, int, int> _rollDie;

        public (int d1, int d2) LastRoll { get; private set; }

        public DiceManager(Func<int, int, int> rollDie = null)
        {
            _rollDie = rollDie ?? ((min, max) => UnityEngine.Random.Range(min, max));
        }

        public int Roll()
        {
            int d1 = _rollDie(1, 7);
            int d2 = _rollDie(1, 7);
            LastRoll = (d1, d2);
            EventBus.Publish(new DiceRolledEvent { D1 = d1, D2 = d2, Total = d1 + d2 });
            return d1 + d2;
        }
    }
}
