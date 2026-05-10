using GameCore.Player;

namespace GameCore.Build
{
    public interface IBuildRule
    {
        bool CanPlace(IPlaceable piece, IBuildLocation location, IPlayer player);
        bool CanRemove(IPlaceable piece, IBuildLocation location, IPlayer player);
        string GetFailureReason(IPlaceable piece, IBuildLocation location, IPlayer player);
    }
}
