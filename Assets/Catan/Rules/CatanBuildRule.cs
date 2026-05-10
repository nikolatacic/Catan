using GameCore.Build;
using GameCore.Player;

namespace Catan
{
    public class CatanBuildRule : IBuildRule
    {
        private CatanBoard _board;

        public CatanBuildRule(CatanBoard board) => _board = board;

        public bool CanPlace(IPlaceable piece, IBuildLocation location, IPlayer player)
            => throw new System.NotImplementedException();

        public bool CanRemove(IPlaceable piece, IBuildLocation location, IPlayer player)
            => throw new System.NotImplementedException();

        public string GetFailureReason(IPlaceable piece, IBuildLocation location, IPlayer player)
            => throw new System.NotImplementedException();
    }
}
