using GameCore.Resources;

namespace GameCore.Build
{
    public interface IPlaceable
    {
        string PlaceableId { get; }
        ResourceBundle BuildCost { get; }
    }
}
