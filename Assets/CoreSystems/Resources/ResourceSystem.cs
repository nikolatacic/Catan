using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic resource/inventory management system.
/// Uses CardSystem internally (hybrid approach) for card-level tracking while providing a simple resource interface.
/// </summary>
public class ResourceSystem : IResourceSystem
{
    private ResourceInventory inventory;

    /// <summary>
    /// Create a new ResourceSystem instance.
    /// </summary>
    public ResourceSystem()
    {
        inventory = new ResourceInventory();
    }

    /// <summary>
    /// Add resources to inventory.
    /// </summary>
    public void AddResource(string resourceType, int count)
    {
        if (count < 0)
        {
            Debug.LogWarning($"ResourceSystem: Cannot add negative count ({count})");
            return;
        }

        inventory.AddResource(resourceType, count);
        EventBus.Publish(new ResourceAddedEvent(resourceType, count));
        EventBus.Publish(new ResourceChangedEvent(resourceType, inventory.GetResourceCount(resourceType)));
    }

    /// <summary>
    /// Remove resources from inventory.
    /// </summary>
    public bool RemoveResource(string resourceType, int count)
    {
        if (count < 0)
        {
            Debug.LogWarning($"ResourceSystem: Cannot remove negative count ({count})");
            return false;
        }

        bool success = inventory.RemoveResource(resourceType, count);

        if (success)
        {
            EventBus.Publish(new ResourceRemovedEvent(resourceType, count));
            EventBus.Publish(new ResourceChangedEvent(resourceType, inventory.GetResourceCount(resourceType)));
        }
        else
        {
            Debug.LogWarning($"ResourceSystem: Cannot remove {count} {resourceType}, insufficient resources");
        }

        return success;
    }

    /// <summary>
    /// Check if inventory has the specified amount of resources.
    /// </summary>
    public bool HasResource(string resourceType, int count)
    {
        return inventory.HasResource(resourceType, count);
    }

    /// <summary>
    /// Get the count of a specific resource type.
    /// </summary>
    public int GetResourceCount(string resourceType)
    {
        return inventory.GetResourceCount(resourceType);
    }

    /// <summary>
    /// Get all resource types and their counts.
    /// </summary>
    public Dictionary<string, int> GetAllResources()
    {
        return inventory.GetAllResources();
    }

    /// <summary>
    /// Transfer resources from this inventory to another.
    /// </summary>
    public bool TransferResource(IResourceSystem target, string resourceType, int count)
    {
        if (target == null)
        {
            Debug.LogWarning("ResourceSystem: Cannot transfer to null target");
            return false;
        }

        if (!HasResource(resourceType, count))
        {
            return false;
        }

        if (!RemoveResource(resourceType, count))
        {
            return false;
        }

        target.AddResource(resourceType, count);
        return true;
    }

    /// <summary>
    /// Create a resource deck (for bank/supply).
    /// Creates a deck of identical cards for the specified resource type.
    /// </summary>
    public void CreateResourceDeck(string resourceType, int cardCount)
    {
        if (cardCount < 0)
        {
            Debug.LogWarning($"ResourceSystem: Cannot create deck with negative count ({cardCount})");
            return;
        }

        inventory.CreateResourceDeck(resourceType, cardCount);
        Debug.Log($"ResourceSystem: Created deck for {resourceType} with {cardCount} cards");
    }

    /// <summary>
    /// Draw resources from a resource deck (bank/supply).
    /// </summary>
    public bool DrawFromDeck(string resourceType, int count)
    {
        if (count < 0)
        {
            Debug.LogWarning($"ResourceSystem: Cannot draw negative count ({count})");
            return false;
        }

        bool success = inventory.DrawFromDeck(resourceType, count);

        if (success)
        {
            EventBus.Publish(new ResourceAddedEvent(resourceType, count));
            EventBus.Publish(new ResourceChangedEvent(resourceType, inventory.GetResourceCount(resourceType)));
        }

        return success;
    }

    /// <summary>
    /// Get the number of resources available in a deck (bank/supply).
    /// </summary>
    public int GetDeckCount(string resourceType)
    {
        return inventory.GetDeckCount(resourceType);
    }

    /// <summary>
    /// Reset the resource system, clearing all inventories and decks.
    /// </summary>
    public void Reset()
    {
        inventory.Reset();
    }

    /// <summary>
    /// Get the internal ResourceInventory (for advanced operations).
    /// </summary>
    public ResourceInventory GetInventory()
    {
        return inventory;
    }
}

