using UnityEngine;

/// <summary>
/// Event published when a card is discarded.
/// </summary>
public class CardDiscardedEvent : IEvent
{
    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public float Timestamp { get; }

    /// <summary>
    /// The card that was discarded.
    /// </summary>
    public ICard Card { get; }

    /// <summary>
    /// Create a new CardDiscardedEvent.
    /// </summary>
    /// <param name="card">The card that was discarded</param>
    public CardDiscardedEvent(ICard card)
    {
        if (card == null)
        {
            throw new System.ArgumentNullException(nameof(card), "Card cannot be null");
        }

        Card = card;
        Timestamp = Time.time;
    }
}

