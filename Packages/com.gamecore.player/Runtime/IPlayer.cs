using UnityEngine;

namespace GameCore.Player
{
    public interface IPlayer
    {
        string Id { get; }
        string DisplayName { get; }
        Color Color { get; }
    }
}
