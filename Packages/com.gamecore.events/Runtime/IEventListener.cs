namespace GameCore.Events
{
    public interface IEventListener<T> where T : IGameEvent
    {
        void OnEventRaised(T value);
    }
}
