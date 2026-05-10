namespace GameCore.Turn
{
    public interface ITurnActor
    {
        string Id { get; }
        bool CanAct { get; }
    }
}
