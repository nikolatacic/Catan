using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Catan-specific resource system that wraps the generic ResourceSystem.
/// Manages Catan resources (Wood, Sheep, Brick, Wheat, Ore) with Catan-specific rules.
/// </summary>
public class CatanResourceSystem
{
    private IResourceSystem resourceSystem;

    /// <summary>
    /// Standard Catan resource deck sizes (bank supply).
    /// </summary>
    private static readonly Dictionary<ResourceType, int> StandardDeckSizes = new Dictionary<ResourceType, int>
    {
        { ResourceType.Wood, 19 },
        { ResourceType.Sheep, 19 },
        { ResourceType.Brick, 19 },
        { ResourceType.Wheat, 19 },
        { ResourceType.Ore, 19 }
    };

    /// <summary>
    /// Create a new CatanResourceSystem instance.
    /// </summary>
    /// <param name="autoRegister">Whether to automatically register with ServiceLocator. Default: true.</param>
    public CatanResourceSystem(bool autoRegister = true)
    {
        resourceSystem = new ResourceSystem();

        if (autoRegister)
        {
            ServiceLocator.Register<CatanResourceSystem>(this);
        }
    }

    /// <summary>
    /// Initialize the Catan resource system with standard bank decks.
    /// </summary>
    public void InitializeBank()
    {
        foreach (KeyValuePair<ResourceType, int> entry in StandardDeckSizes)
        {
            if (entry.Key != ResourceType.Desert)
            {
                resourceSystem.CreateResourceDeck(entry.Key.ToString(), entry.Value);
            }
        }

        Debug.Log("CatanResourceSystem: Bank initialized with standard resource decks");
    }

    /// <summary>
    /// Add resources to player inventory.
    /// </summary>
    /// <param name="resourceType">Type of resource to add</param>
    /// <param name="count">Number of resources to add</param>
    public void AddResource(ResourceType resourceType, int count)
    {
        if (resourceType == ResourceType.Desert)
        {
            Debug.LogWarning("CatanResourceSystem: Cannot add Desert as a resource");
            return;
        }

        resourceSystem.AddResource(resourceType.ToString(), count);
    }

    /// <summary>
    /// Remove resources from player inventory.
    /// </summary>
    /// <param name="resourceType">Type of resource to remove</param>
    /// <param name="count">Number of resources to remove</param>
    /// <returns>True if removal was successful</returns>
    public bool RemoveResource(ResourceType resourceType, int count)
    {
        if (resourceType == ResourceType.Desert)
        {
            Debug.LogWarning("CatanResourceSystem: Cannot remove Desert as a resource");
            return false;
        }

        return resourceSystem.RemoveResource(resourceType.ToString(), count);
    }

    /// <summary>
    /// Check if player has the specified amount of resources.
    /// </summary>
    /// <param name="resourceType">Type of resource to check</param>
    /// <param name="count">Required count</param>
    /// <returns>True if player has at least the required count</returns>
    public bool HasResource(ResourceType resourceType, int count)
    {
        if (resourceType == ResourceType.Desert)
        {
            return false;
        }

        return resourceSystem.HasResource(resourceType.ToString(), count);
    }

    /// <summary>
    /// Get the count of a specific resource type.
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <returns>Number of resources of this type</returns>
    public int GetResourceCount(ResourceType resourceType)
    {
        if (resourceType == ResourceType.Desert)
        {
            return 0;
        }

        return resourceSystem.GetResourceCount(resourceType.ToString());
    }

    /// <summary>
    /// Get all resource counts as a dictionary.
    /// </summary>
    /// <returns>Dictionary of resource types and their counts</returns>
    public Dictionary<ResourceType, int> GetAllResources()
    {
        Dictionary<ResourceType, int> result = new Dictionary<ResourceType, int>();

        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (type != ResourceType.Desert)
            {
                result[type] = GetResourceCount(type);
            }
        }

        return result;
    }

    /// <summary>
    /// Draw resources from the bank (supply).
    /// </summary>
    /// <param name="resourceType">Type of resource to draw</param>
    /// <param name="count">Number of resources to draw</param>
    /// <returns>True if draw was successful</returns>
    public bool DrawFromBank(ResourceType resourceType, int count)
    {
        if (resourceType == ResourceType.Desert)
        {
            Debug.LogWarning("CatanResourceSystem: Cannot draw Desert from bank");
            return false;
        }

        return resourceSystem.DrawFromDeck(resourceType.ToString(), count);
    }

    /// <summary>
    /// Get the number of resources available in the bank.
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <returns>Number of resources available in bank</returns>
    public int GetBankCount(ResourceType resourceType)
    {
        if (resourceType == ResourceType.Desert)
        {
            return 0;
        }

        return resourceSystem.GetDeckCount(resourceType.ToString());
    }

    /// <summary>
    /// Calculate resource distribution based on dice roll.
    /// Returns a dictionary of what should be distributed to each player.
    /// </summary>
    /// <param name="diceNumber">The dice number rolled (2-12, not 7)</param>
    /// <param name="fieldResourceMap">Dictionary mapping field positions to resource types</param>
    /// <param name="playerSettlements">Dictionary mapping field positions to player IDs who have settlements there</param>
    /// <returns>Dictionary mapping player IDs to dictionaries of resource types and counts to distribute</returns>
    public Dictionary<int, Dictionary<ResourceType, int>> CalculateResourceDistribution(int diceNumber, Dictionary<int, ResourceType> fieldResourceMap, Dictionary<int, List<int>> playerSettlements)
    {
        Dictionary<int, Dictionary<ResourceType, int>> distribution = new Dictionary<int, Dictionary<ResourceType, int>>();

        if (diceNumber == 7)
        {
            Debug.LogWarning("CatanResourceSystem: Cannot distribute resources on 7 (robber activation)");
            return distribution;
        }

        if (fieldResourceMap == null || playerSettlements == null)
        {
            Debug.LogWarning("CatanResourceSystem: Cannot calculate distribution, missing field data");
            return distribution;
        }

        // Find all fields with this dice number
        foreach (KeyValuePair<int, ResourceType> fieldEntry in fieldResourceMap)
        {
            int fieldPosition = fieldEntry.Key;
            ResourceType resourceType = fieldEntry.Value;

            // Skip desert fields
            if (resourceType == ResourceType.Desert)
            {
                continue;
            }

            // Check if any players have settlements on this field
            if (playerSettlements.ContainsKey(fieldPosition))
            {
                List<int> players = playerSettlements[fieldPosition];

                // Distribute resources to each player
                foreach (int playerID in players)
                {
                    // Initialize player distribution if needed
                    if (!distribution.ContainsKey(playerID))
                    {
                        distribution[playerID] = new Dictionary<ResourceType, int>();
                    }

                    // For now, we'll distribute 1 resource per settlement
                    // In a full implementation, this would check if it's a city (2 resources) or settlement (1 resource)
                    if (!distribution[playerID].ContainsKey(resourceType))
                    {
                        distribution[playerID][resourceType] = 0;
                    }

                    distribution[playerID][resourceType]++;
                }
            }
        }

        return distribution;
    }

    /// <summary>
    /// Distribute resources to a specific player from the bank.
    /// </summary>
    /// <param name="playerID">ID of the player receiving resources</param>
    /// <param name="resourceType">Type of resource to distribute</param>
    /// <param name="count">Number of resources to distribute</param>
    /// <param name="diceNumber">The dice number that triggered distribution</param>
    /// <returns>True if distribution was successful</returns>
    public bool DistributeToPlayer(int playerID, ResourceType resourceType, int count, int diceNumber)
    {
        if (diceNumber == 7)
        {
            Debug.LogWarning("CatanResourceSystem: Cannot distribute resources on 7 (robber activation)");
            return false;
        }

        if (resourceType == ResourceType.Desert)
        {
            return false;
        }

        // Draw from bank and add to this player's inventory
        bool success = DrawFromBank(resourceType, count);

        if (success)
        {
            EventBus.Publish(new CatanResourceDistributedEvent(playerID, resourceType, count, diceNumber));
        }
        else
        {
            Debug.LogWarning($"CatanResourceSystem: Bank ran out of {resourceType}");
        }

        return success;
    }

    /// <summary>
    /// Transfer resources between players.
    /// </summary>
    /// <param name="target">Target Catan resource system</param>
    /// <param name="resourceType">Type of resource to transfer</param>
    /// <param name="count">Number of resources to transfer</param>
    /// <returns>True if transfer was successful</returns>
    public bool TransferResource(CatanResourceSystem target, ResourceType resourceType, int count)
    {
        if (target == null)
        {
            Debug.LogWarning("CatanResourceSystem: Cannot transfer to null target");
            return false;
        }

        if (resourceType == ResourceType.Desert)
        {
            Debug.LogWarning("CatanResourceSystem: Cannot transfer Desert");
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
    /// Reset the resource system.
    /// </summary>
    public void Reset()
    {
        resourceSystem.Reset();
    }

    /// <summary>
    /// Unregister from ServiceLocator.
    /// </summary>
    public void Unregister()
    {
        ServiceLocator.Unregister<CatanResourceSystem>();
    }

    /// <summary>
    /// Get the underlying ResourceSystem (for advanced operations).
    /// </summary>
    public IResourceSystem GetResourceSystem()
    {
        return resourceSystem;
    }
}

