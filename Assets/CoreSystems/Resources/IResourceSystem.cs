using System.Collections.Generic;

/// <summary>
/// Interface for resource/inventory management system.
/// Provides a simple interface for resource operations while using CardSystem internally.
/// </summary>
public interface IResourceSystem
{
    /// <summary>
    /// Add resources to inventory.
    /// </summary>
    /// <param name="resourceType">Type of resource to add</param>
    /// <param name="count">Number of resources to add</param>
    void AddResource(string resourceType, int count);

    /// <summary>
    /// Remove resources from inventory.
    /// </summary>
    /// <param name="resourceType">Type of resource to remove</param>
    /// <param name="count">Number of resources to remove</param>
    /// <returns>True if removal was successful, false if insufficient resources</returns>
    bool RemoveResource(string resourceType, int count);

    /// <summary>
    /// Check if inventory has the specified amount of resources.
    /// </summary>
    /// <param name="resourceType">Type of resource to check</param>
    /// <param name="count">Required count</param>
    /// <returns>True if inventory has at least the required count</returns>
    bool HasResource(string resourceType, int count);

    /// <summary>
    /// Get the count of a specific resource type.
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <returns>Number of resources of this type</returns>
    int GetResourceCount(string resourceType);

    /// <summary>
    /// Get all resource types and their counts.
    /// </summary>
    /// <returns>Dictionary of resource types and their counts</returns>
    Dictionary<string, int> GetAllResources();

    /// <summary>
    /// Transfer resources from this inventory to another.
    /// </summary>
    /// <param name="target">Target resource system</param>
    /// <param name="resourceType">Type of resource to transfer</param>
    /// <param name="count">Number of resources to transfer</param>
    /// <returns>True if transfer was successful</returns>
    bool TransferResource(IResourceSystem target, string resourceType, int count);

    /// <summary>
    /// Create a resource deck (for bank/supply).
    /// Creates a deck of identical cards for the specified resource type.
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <param name="cardCount">Number of cards in the deck</param>
    void CreateResourceDeck(string resourceType, int cardCount);

    /// <summary>
    /// Draw resources from a resource deck (bank/supply).
    /// </summary>
    /// <param name="resourceType">Type of resource to draw</param>
    /// <param name="count">Number of resources to draw</param>
    /// <returns>True if draw was successful</returns>
    bool DrawFromDeck(string resourceType, int count);

    /// <summary>
    /// Get the number of resources available in a deck (bank/supply).
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <returns>Number of resources available in deck</returns>
    int GetDeckCount(string resourceType);

    /// <summary>
    /// Reset the resource system, clearing all inventories and decks.
    /// </summary>
    void Reset();
}

