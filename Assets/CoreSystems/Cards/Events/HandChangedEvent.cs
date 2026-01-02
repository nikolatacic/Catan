using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Event published when the hand changes (card added or removed).
/// </summary>
public class HandChangedEvent : IEvent
{
    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public float Timestamp { get; }

    /// <summary>
    /// The current hand (new list, safe to modify).
    /// </summary>
    public List<ICard> Hand { get; }

    /// <summary>
    /// Number of cards in the hand.
    /// </summary>
    public int HandCount { get; }

    /// <summary>
    /// Create a new HandChangedEvent.
    /// </summary>
    /// <param name="hand">The current hand</param>
    public HandChangedEvent(List<ICard> hand)
    {
        if (hand == null)
        {
            Hand = new List<ICard>();
        }
        else
        {
            Hand = new List<ICard>(hand);
        }

        HandCount = Hand.Count;
        Timestamp = Time.time;
    }
}

