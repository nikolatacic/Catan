using UnityEngine;
using GameCore.Resources;

namespace Catan
{
    [CreateAssetMenu(menuName = "Catan/Resource")]
    public class CatanResource : ScriptableObject, IResource
    {
        [SerializeField] private CatanResourceType _type;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _color = Color.white;

        public CatanResourceType Type => _type;
        public Sprite Icon => _icon;
        public Color Color => _color;
        public string ResourceId => _type.ToString();
        public string DisplayName => _type.ToString();
    }
}
