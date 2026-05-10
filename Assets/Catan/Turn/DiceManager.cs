using GameCore.Events;

namespace Catan
{
    public class DiceManager
    {
        public (int d1, int d2) LastRoll { get; private set; }

        public int Roll()
        {
            var d1 = UnityEngine.Random.Range(1, 7);
            var d2 = UnityEngine.Random.Range(1, 7);
            LastRoll = (d1, d2);
            EventBus.Publish(new DiceRolledEvent { D1 = d1, D2 = d2, Total = d1 + d2 });
            return d1 + d2;
        }
    }
}
