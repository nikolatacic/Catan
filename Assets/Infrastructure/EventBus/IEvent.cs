/// <summary>
/// Base interface for all events in the event system.
/// All events must implement this interface to be used with EventBus.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Timestamp when the event was created (in seconds since game start).
    /// </summary>
    float Timestamp { get; }
}

