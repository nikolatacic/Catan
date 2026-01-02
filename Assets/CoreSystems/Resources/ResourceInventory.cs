using System.Collections.Generic;

/// <summary>
/// Represents a resource inventory that uses CardSystem internally.
/// Provides a simple interface for resource management while maintaining card-level tracking.
/// </summary>
public class ResourceInventory
{
    private Dictionary<string, ICardSystem> resourceDecks; // One CardSystem per resource type (for bank/supply)
    private Dictionary<string, ICardSystem> inventories; // One CardSystem per resource type (for player inventories)

    /// <summary>
    /// Create a new ResourceInventory instance.
    /// </summary>
    public ResourceInventory()
    {
        resourceDecks = new Dictionary<string, ICardSystem>();
        inventories = new Dictionary<string, ICardSystem>();
    }

    /// <summary>
    /// Get or create a CardSystem for a resource type in inventory.
    /// </summary>
    private ICardSystem GetOrCreateInventory(string resourceType)
    {
        if (!inventories.ContainsKey(resourceType))
        {
            inventories[resourceType] = new CardSystem();
        }
        return inventories[resourceType];
    }

    /// <summary>
    /// Get or create a CardSystem for a resource type in deck.
    /// </summary>
    private ICardSystem GetOrCreateDeck(string resourceType)
    {
        if (!resourceDecks.ContainsKey(resourceType))
        {
            resourceDecks[resourceType] = new CardSystem();
        }
        return resourceDecks[resourceType];
    }

    /// <summary>
    /// Add resources to inventory.
    /// </summary>
    public void AddResource(string resourceType, int count)
    {
        ICardSystem inventory = GetOrCreateInventory(resourceType);

        // Create cards and add to hand
        for (int i = 0; i < count; i++)
        {
            CardData card = new CardData(resourceType, resourceType);
            inventory.AddToHand(card);
        }
    }

    /// <summary>
    /// Remove resources from inventory.
    /// </summary>
    public bool RemoveResource(string resourceType, int count)
    {
        if (!inventories.ContainsKey(resourceType))
        {
            return false;
        }

        ICardSystem inventory = inventories[resourceType];
        int availableCount = inventory.GetHandCount();

        if (availableCount < count)
        {
            return false;
        }

        // Remove cards from hand by discarding them
        // Get hand and discard first N cards
        for (int i = 0; i < count; i++)
        {
            List<ICard> hand = inventory.GetHand();
            if (hand.Count == 0)
            {
                break; // Should not happen, but safety check
            }
            inventory.DiscardCard(hand[0]); // Always discard first card
        }

        return true;
    }

    /// <summary>
    /// Check if inventory has the specified amount of resources.
    /// </summary>
    public bool HasResource(string resourceType, int count)
    {
        if (!inventories.ContainsKey(resourceType))
        {
            return false;
        }

        return inventories[resourceType].GetHandCount() >= count;
    }

    /// <summary>
    /// Get the count of a specific resource type.
    /// </summary>
    public int GetResourceCount(string resourceType)
    {
        if (!inventories.ContainsKey(resourceType))
        {
            return 0;
        }

        return inventories[resourceType].GetHandCount();
    }

    /// <summary>
    /// Get all resource types and their counts.
    /// </summary>
    public Dictionary<string, int> GetAllResources()
    {
        Dictionary<string, int> result = new Dictionary<string, int>();

        foreach (KeyValuePair<string, ICardSystem> entry in inventories)
        {
            result[entry.Key] = entry.Value.GetHandCount();
        }

        return result;
    }

    /// <summary>
    /// Create a resource deck (for bank/supply).
    /// </summary>
    public void CreateResourceDeck(string resourceType, int cardCount)
    {
        ICardSystem deck = GetOrCreateDeck(resourceType);
        deck.Reset();

        // Create config with identical cards
        CardConfig config = UnityEngine.ScriptableObject.CreateInstance<CardConfig>();
        CardConfig.CardTypeEntry entry = new CardConfig.CardTypeEntry
        {
            cardType = resourceType,
            cardName = resourceType,
            count = cardCount
        };
        config.CardTypes = new List<CardConfig.CardTypeEntry> { entry };

        deck.CreateDeck(config);
    }

    /// <summary>
    /// Draw resources from a resource deck.
    /// </summary>
    public bool DrawFromDeck(string resourceType, int count)
    {
        if (!resourceDecks.ContainsKey(resourceType))
        {
            return false;
        }

        ICardSystem deck = resourceDecks[resourceType];
        int availableCount = deck.GetDeckCount();

        if (availableCount < count)
        {
            return false;
        }

        // Draw cards and add to inventory
        for (int i = 0; i < count; i++)
        {
            ICard card = deck.DrawCard();
            if (card != null)
            {
                AddResource(resourceType, 1);
            }
        }

        return true;
    }

    /// <summary>
    /// Get the number of resources available in a deck.
    /// </summary>
    public int GetDeckCount(string resourceType)
    {
        if (!resourceDecks.ContainsKey(resourceType))
        {
            return 0;
        }

        return resourceDecks[resourceType].GetDeckCount();
    }

    /// <summary>
    /// Transfer resources from this inventory to another.
    /// </summary>
    public bool TransferResource(ResourceInventory target, string resourceType, int count)
    {
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
    /// Reset the inventory, clearing all resources and decks.
    /// </summary>
    public void Reset()
    {
        foreach (ICardSystem inventory in inventories.Values)
        {
            inventory.Reset();
        }

        foreach (ICardSystem deck in resourceDecks.Values)
        {
            deck.Reset();
        }

        inventories.Clear();
        resourceDecks.Clear();
    }

    /// <summary>
    /// Get the internal CardSystem for a resource type (for advanced operations).
    /// </summary>
    public ICardSystem GetInventoryCardSystem(string resourceType)
    {
        return inventories.ContainsKey(resourceType) ? inventories[resourceType] : null;
    }

    /// <summary>
    /// Get the internal CardSystem for a resource deck (for advanced operations).
    /// </summary>
    public ICardSystem GetDeckCardSystem(string resourceType)
    {
        return resourceDecks.ContainsKey(resourceType) ? resourceDecks[resourceType] : null;
    }
}

