using UnityEngine;

/// <summary>
/// Event published when a Catan development card is drawn.
/// Contains Catan-specific card information.
/// </summary>
public class CatanCardDrawnEvent : IEvent
{
    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public float Timestamp { get; }

    /// <summary>
    /// The Catan card that was drawn.
    /// </summary>
    public CatanCardData Card { get; }

    /// <summary>
    /// The underlying generic card drawn event.
    /// </summary>
    public CardDrawnEvent BaseEvent { get; }

    /// <summary>
    /// Create a new CatanCardDrawnEvent.
    /// </summary>
    /// <param name="card">The Catan card that was drawn</param>
    /// <param name="baseEvent">The underlying generic card drawn event</param>
    public CatanCardDrawnEvent(CatanCardData card, CardDrawnEvent baseEvent)
    {
        if (card == null)
        {
            throw new System.ArgumentNullException(nameof(card), "Card cannot be null");
        }

        Card = card;
        BaseEvent = baseEvent;
        Timestamp = Time.time;
    }
}

