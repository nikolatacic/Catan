using System.Collections.Generic;

namespace GameCore.Resources
{
    public class ResourceBundle
    {
        private readonly Dictionary<IResource, int> _amounts;

        public ResourceBundle() => _amounts = new Dictionary<IResource, int>();

        private ResourceBundle(Dictionary<IResource, int> amounts) => _amounts = amounts;

        public int Get(IResource res) => _amounts.TryGetValue(res, out var v) ? v : 0;

        public ResourceBundle Add(IResource res, int amount)
        {
            var copy = new Dictionary<IResource, int>(_amounts);
            copy[res] = Get(res) + amount;
            return new ResourceBundle(copy);
        }

        public ResourceBundle Remove(IResource res, int amount)
        {
            var copy = new Dictionary<IResource, int>(_amounts);
            copy[res] = System.Math.Max(0, Get(res) - amount);
            return new ResourceBundle(copy);
        }

        public bool CanAfford(ResourceBundle cost)
        {
            foreach (var pair in cost._amounts)
                if (Get(pair.Key) < pair.Value) return false;
            return true;
        }

        public IReadOnlyDictionary<IResource, int> Amounts => _amounts;

        public static ResourceBundle operator +(ResourceBundle a, ResourceBundle b)
        {
            var result = a;
            foreach (var pair in b._amounts)
                result = result.Add(pair.Key, pair.Value);
            return result;
        }
    }
}
