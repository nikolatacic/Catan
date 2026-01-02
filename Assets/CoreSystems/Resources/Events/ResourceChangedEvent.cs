using UnityEngine;

/// <summary>
/// Event published when the count of a resource type changes.
/// </summary>
public class ResourceChangedEvent : IEvent
{
    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public float Timestamp { get; }

    /// <summary>
    /// Type of resource that changed.
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// New count of the resource.
    /// </summary>
    public int NewCount { get; }

    /// <summary>
    /// Create a new ResourceChangedEvent.
    /// </summary>
    /// <param name="resourceType">Type of resource that changed</param>
    /// <param name="newCount">New count of the resource</param>
    public ResourceChangedEvent(string resourceType, int newCount)
    {
        if (string.IsNullOrEmpty(resourceType))
        {
            throw new System.ArgumentNullException(nameof(resourceType), "Resource type cannot be null or empty");
        }

        ResourceType = resourceType;
        NewCount = newCount;
        Timestamp = Time.time;
    }
}

