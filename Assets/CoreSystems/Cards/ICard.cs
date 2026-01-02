/// <summary>
/// Interface for card objects.
/// Provides a generic contract for any card implementation.
/// </summary>
public interface ICard
{
    /// <summary>
    /// Unique identifier for the card.
    /// </summary>
    int CardID { get; }

    /// <summary>
    /// Type of the card (can be string, enum, or any identifier).
    /// </summary>
    string CardType { get; }

    /// <summary>
    /// Name of the card.
    /// </summary>
    string CardName { get; }
}

