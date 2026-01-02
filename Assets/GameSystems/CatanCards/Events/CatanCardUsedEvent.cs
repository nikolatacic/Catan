using UnityEngine;

/// <summary>
/// Event published when a Catan development card is used/activated.
/// </summary>
public class CatanCardUsedEvent : IEvent
{
    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public float Timestamp { get; }

    /// <summary>
    /// The Catan card that was used.
    /// </summary>
    public CatanCardData Card { get; }

    /// <summary>
    /// ID of the player who used the card.
    /// </summary>
    public int PlayerID { get; }

    /// <summary>
    /// Create a new CatanCardUsedEvent.
    /// </summary>
    /// <param name="card">The Catan card that was used</param>
    /// <param name="playerID">ID of the player who used the card</param>
    public CatanCardUsedEvent(CatanCardData card, int playerID)
    {
        if (card == null)
        {
            throw new System.ArgumentNullException(nameof(card), "Card cannot be null");
        }

        Card = card;
        PlayerID = playerID;
        Timestamp = Time.time;
    }
}

