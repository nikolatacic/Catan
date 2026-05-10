using UnityEngine;

namespace GameCore.Player
{
    [CreateAssetMenu(menuName = "GameCore/Player Data")]
    public class PlayerData : ScriptableObject, IPlayer
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Color _color;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Color Color => _color;
    }
}
