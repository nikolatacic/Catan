using UnityEngine;
using GameCore.Resources;

namespace Catan
{
    [CreateAssetMenu(menuName = "Catan/Resource")]
    public class CatanResource : ScriptableObject, IResource
    {
        [SerializeField] private CatanResourceType _type;

        public CatanResourceType Type => _type;
        public string ResourceId => _type.ToString();
        public string DisplayName => _type.ToString();
    }
}
