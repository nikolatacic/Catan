using UnityEngine;
using GameCore.Player;

namespace GameCore.Build
{
    public class BuildManager : MonoBehaviour
    {
        public IBuildRule Rule { get; set; }

        public bool TryPlace(IPlaceable piece, IBuildLocation location, IPlayer player)
            => throw new System.NotImplementedException();

        public bool TryRemove(IPlaceable piece, IBuildLocation location)
            => throw new System.NotImplementedException();
    }
}
