using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Catan-specific card system that wraps the generic CardSystem.
/// Manages Catan development cards with Catan-specific rules.
/// </summary>
public class CatanCardSystem
{
    private ICardSystem cardSystem;
    private Dictionary<int, CatanCardData> cardMapping; // Maps generic card ID to Catan card
    private Dictionary<CatanCardData, int> reverseMapping; // Maps Catan card to generic card ID
    private List<CatanCardData> availableCatanCards; // Available Catan cards not yet drawn

    /// <summary>
    /// Standard Catan development card distribution.
    /// </summary>
    private static readonly Dictionary<CatanCardType, int> StandardDeckDistribution = new Dictionary<CatanCardType, int>
    {
        { CatanCardType.Knight, 14 },
        { CatanCardType.RoadBuilding, 2 },
        { CatanCardType.YearOfPlenty, 2 },
        { CatanCardType.Monopoly, 2 },
        { CatanCardType.VictoryPoint, 5 }
    };

    /// <summary>
    /// Create a new CatanCardSystem instance.
    /// </summary>
    /// <param name="autoRegister">Whether to automatically register with ServiceLocator. Default: true.</param>
    public CatanCardSystem(bool autoRegister = true)
    {
        cardSystem = new CardSystem();
        cardMapping = new Dictionary<int, CatanCardData>();
        reverseMapping = new Dictionary<CatanCardData, int>();
        availableCatanCards = new List<CatanCardData>();

        if (autoRegister)
        {
            ServiceLocator.Register<CatanCardSystem>(this);
        }
    }

    /// <summary>
    /// Create the standard Catan development card deck.
    /// </summary>
    public void CreateDevelopmentCardDeck()
    {
        cardMapping.Clear();
        reverseMapping.Clear();
        availableCatanCards.Clear();
        cardSystem.Reset();

        // Create Catan cards
        List<CatanCardData> catanCards = new List<CatanCardData>();
        foreach (KeyValuePair<CatanCardType, int> entry in StandardDeckDistribution)
        {
            for (int i = 0; i < entry.Value; i++)
            {
                CatanCardData card = new CatanCardData(entry.Key);
                catanCards.Add(card);
            }
        }

        // Create config and deck in CardSystem
        CardConfig config = CreateConfigFromCatanCards(catanCards);
        cardSystem.CreateDeck(config);
        cardSystem.ShuffleDeck();

        // Build mapping: match generic cards to Catan cards by type
        // We'll match them as they're drawn since CardSystem creates new instances
        availableCatanCards = new List<CatanCardData>(catanCards);

        Debug.Log($"CatanCardSystem: Created development card deck with {catanCards.Count} cards");
    }

    /// <summary>
    /// Create a CardConfig from Catan cards.
    /// </summary>
    private CardConfig CreateConfigFromCatanCards(List<CatanCardData> catanCards)
    {
        CardConfig config = ScriptableObject.CreateInstance<CardConfig>();
        List<CardConfig.CardTypeEntry> entries = new List<CardConfig.CardTypeEntry>();

        // Group cards by type
        Dictionary<CatanCardType, int> typeCounts = new Dictionary<CatanCardType, int>();
        foreach (CatanCardData card in catanCards)
        {
            if (!typeCounts.ContainsKey(card.CatanType))
            {
                typeCounts[card.CatanType] = 0;
            }
            typeCounts[card.CatanType]++;
        }

        // Create entries
        foreach (KeyValuePair<CatanCardType, int> entry in typeCounts)
        {
            CardConfig.CardTypeEntry configEntry = new CardConfig.CardTypeEntry
            {
                cardType = entry.Key.ToString(),
                cardName = GetCardName(entry.Key),
                count = entry.Value
            };
            entries.Add(configEntry);
        }

        config.CardTypes = entries;
        return config;
    }

    /// <summary>
    /// Get display name for Catan card type.
    /// </summary>
    private string GetCardName(CatanCardType type)
    {
        switch (type)
        {
            case CatanCardType.Knight:
                return "Knight";
            case CatanCardType.RoadBuilding:
                return "Road Building";
            case CatanCardType.YearOfPlenty:
                return "Year of Plenty";
            case CatanCardType.Monopoly:
                return "Monopoly";
            case CatanCardType.VictoryPoint:
                return "Victory Point";
            default:
                return type.ToString();
        }
    }

    /// <summary>
    /// Draw a development card.
    /// </summary>
    /// <returns>CatanCardDrawnEvent with the drawn card, or null if deck is empty</returns>
    public CatanCardDrawnEvent DrawDevelopmentCard()
    {
        ICard genericCard = cardSystem.DrawCard();

        if (genericCard == null)
        {
            Debug.LogWarning("CatanCardSystem: Cannot draw card, deck is empty");
            return null;
        }

        // Find matching Catan card by type from available cards
        CatanCardType drawnType = ParseCardType(genericCard.CardType);
        CatanCardData catanCard = availableCatanCards.FirstOrDefault(c => c.CatanType == drawnType);

        if (catanCard == null)
        {
            Debug.LogError($"CatanCardSystem: Could not find Catan card for type {genericCard.CardType}");
            return null;
        }

        // Remove from available and add to mapping (both directions)
        availableCatanCards.Remove(catanCard);
        cardMapping[genericCard.CardID] = catanCard;
        reverseMapping[catanCard] = genericCard.CardID;

        // Create Catan-specific event
        CardDrawnEvent baseEvent = new CardDrawnEvent(genericCard);
        CatanCardDrawnEvent catanEvent = new CatanCardDrawnEvent(catanCard, baseEvent);

        EventBus.Publish(catanEvent);

        return catanEvent;
    }

    /// <summary>
    /// Parse card type string to CatanCardType enum.
    /// </summary>
    private CatanCardType ParseCardType(string cardType)
    {
        if (System.Enum.TryParse<CatanCardType>(cardType, out CatanCardType result))
        {
            return result;
        }

        Debug.LogWarning($"CatanCardSystem: Could not parse card type: {cardType}");
        return CatanCardType.Knight; // Default fallback
    }

    /// <summary>
    /// Use/activate a development card.
    /// </summary>
    /// <param name="card">The card to use</param>
    /// <param name="playerID">ID of the player using the card</param>
    public void UseCard(CatanCardData card, int playerID)
    {
        if (card == null)
        {
            Debug.LogWarning("CatanCardSystem: Cannot use null card");
            return;
        }

        // Find the card in hand (could be CatanCardData or generic CardData)
        List<ICard> hand = cardSystem.GetHand();
        ICard cardInHand = hand.FirstOrDefault(c => 
        {
            // Check if it's the same Catan card
            if (c is CatanCardData catanCard && catanCard == card)
            {
                return true;
            }
            // Or check if it's the mapped generic card
            if (reverseMapping.TryGetValue(card, out int genericID) && c.CardID == genericID)
            {
                return true;
            }
            return false;
        });

        if (cardInHand != null)
        {
            cardSystem.DiscardCard(cardInHand);
        }
        else
        {
            Debug.LogWarning($"CatanCardSystem: Card {card.CardName} not found in hand");
        }

        // Publish Catan-specific event
        CatanCardUsedEvent usedEvent = new CatanCardUsedEvent(card, playerID);
        EventBus.Publish(usedEvent);

        Debug.Log($"CatanCardSystem: Player {playerID} used {card.CardName}");
    }

    /// <summary>
    /// Add a card to hand.
    /// Note: This should be called with the card from DrawDevelopmentCard event.
    /// The card is automatically added to hand when drawn, but this method allows manual addition.
    /// </summary>
    /// <param name="card">Card to add to hand</param>
    public void AddToHand(CatanCardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("CatanCardSystem: Cannot add null card to hand");
            return;
        }

        // Add the Catan card directly (it implements ICard)
        // The CardSystem will store it, and GetHand() will recognize it as CatanCardData
        cardSystem.AddToHand(card);
    }

    /// <summary>
    /// Get all cards in hand as Catan cards.
    /// </summary>
    /// <returns>List of Catan cards in hand</returns>
    public List<CatanCardData> GetHand()
    {
        List<ICard> genericHand = cardSystem.GetHand();
        List<CatanCardData> catanHand = new List<CatanCardData>();

        foreach (ICard card in genericHand)
        {
            // If it's already a CatanCardData, use it directly
            if (card is CatanCardData catanCard)
            {
                catanHand.Add(catanCard);
            }
            // Otherwise, try to find it in the mapping
            else if (cardMapping.TryGetValue(card.CardID, out CatanCardData mappedCard))
            {
                catanHand.Add(mappedCard);
            }
        }

        return catanHand;
    }

    /// <summary>
    /// Get the number of cards remaining in the deck.
    /// </summary>
    public int GetDeckCount()
    {
        return cardSystem.GetDeckCount();
    }

    /// <summary>
    /// Get the number of cards in hand.
    /// </summary>
    public int GetHandCount()
    {
        return cardSystem.GetHandCount();
    }

    /// <summary>
    /// Get the number of cards in discard pile.
    /// </summary>
    public int GetDiscardCount()
    {
        return cardSystem.GetDiscardCount();
    }

    /// <summary>
    /// Reset the card system.
    /// </summary>
    public void Reset()
    {
        cardSystem.Reset();
        cardMapping.Clear();
        reverseMapping.Clear();
        availableCatanCards.Clear();
    }

    /// <summary>
    /// Unregister from ServiceLocator.
    /// </summary>
    public void Unregister()
    {
        ServiceLocator.Unregister<CatanCardSystem>();
    }
}

