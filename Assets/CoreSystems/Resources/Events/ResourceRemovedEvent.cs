using UnityEngine;

/// <summary>
/// Event published when resources are removed from an inventory.
/// </summary>
public class ResourceRemovedEvent : IEvent
{
    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public float Timestamp { get; }

    /// <summary>
    /// Type of resource that was removed.
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// Number of resources removed.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Create a new ResourceRemovedEvent.
    /// </summary>
    /// <param name="resourceType">Type of resource removed</param>
    /// <param name="count">Number of resources removed</param>
    public ResourceRemovedEvent(string resourceType, int count)
    {
        if (string.IsNullOrEmpty(resourceType))
        {
            throw new System.ArgumentNullException(nameof(resourceType), "Resource type cannot be null or empty");
        }

        ResourceType = resourceType;
        Count = count;
        Timestamp = Time.time;
    }
}

