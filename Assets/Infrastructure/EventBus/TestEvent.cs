using UnityEngine;

/// <summary>
/// Test event for validating EventBus functionality.
/// This is a simple event that can be used for testing.
/// </summary>
public class TestEvent : IEvent
{
    public float Timestamp { get; }
    public string Message { get; }
    public int Value { get; }

    public TestEvent(string message, int value = 0)
    {
        Timestamp = Time.time;
        Message = message;
        Value = value;
    }
}

