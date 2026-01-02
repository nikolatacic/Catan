using UnityEngine;

/// <summary>
/// Event published when resources are added to an inventory.
/// </summary>
public class ResourceAddedEvent : IEvent
{
    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public float Timestamp { get; }

    /// <summary>
    /// Type of resource that was added.
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// Number of resources added.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Create a new ResourceAddedEvent.
    /// </summary>
    /// <param name="resourceType">Type of resource added</param>
    /// <param name="count">Number of resources added</param>
    public ResourceAddedEvent(string resourceType, int count)
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

