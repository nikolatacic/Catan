using System;

/// <summary>
/// Data class representing a card.
/// Contains card information without game-specific logic.
/// </summary>
[Serializable]
public class CardData : ICard
{
    private static int nextCardID = 1;

    /// <summary>
    /// Unique identifier for the card.
    /// </summary>
    public int CardID { get; private set; }

    /// <summary>
    /// Type of the card (e.g., "Knight", "RoadBuilding", "YearOfPlenty").
    /// </summary>
    public string CardType { get; private set; }

    /// <summary>
    /// Name of the card.
    /// </summary>
    public string CardName { get; private set; }

    /// <summary>
    /// Create a new CardData instance.
    /// </summary>
    /// <param name="cardType">Type of the card</param>
    /// <param name="cardName">Name of the card</param>
    public CardData(string cardType, string cardName = null)
    {
        if (string.IsNullOrEmpty(cardType))
        {
            throw new ArgumentException("Card type cannot be null or empty", nameof(cardType));
        }

        CardID = nextCardID++;
        CardType = cardType;
        CardName = string.IsNullOrEmpty(cardName) ? cardType : cardName;
    }

    /// <summary>
    /// Create a copy of this card.
    /// </summary>
    /// <returns>New CardData instance with same properties</returns>
    public CardData Clone()
    {
        return new CardData(CardType, CardName);
    }

    /// <summary>
    /// Get a string representation of the card.
    /// </summary>
    public override string ToString()
    {
        return $"Card[{CardID}]: {CardName} ({CardType})";
    }
}

