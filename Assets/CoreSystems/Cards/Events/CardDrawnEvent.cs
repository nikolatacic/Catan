using UnityEngine;

/// <summary>
/// Event published when a card is drawn from the deck.
/// </summary>
public class CardDrawnEvent : IEvent
{
    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public float Timestamp { get; }

    /// <summary>
    /// The card that was drawn.
    /// </summary>
    public ICard Card { get; }

    /// <summary>
    /// Create a new CardDrawnEvent.
    /// </summary>
    /// <param name="card">The card that was drawn</param>
    public CardDrawnEvent(ICard card)
    {
        if (card == null)
        {
            throw new System.ArgumentNullException(nameof(card), "Card cannot be null");
        }

        Card = card;
        Timestamp = Time.time;
    }
}

