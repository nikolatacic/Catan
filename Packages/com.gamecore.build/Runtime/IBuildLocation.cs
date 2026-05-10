namespace GameCore.Build
{
    public interface IBuildLocation
    {
        string LocationId { get; }
        bool IsOccupied { get; }
        IPlaceable OccupiedBy { get; }
    }
}
