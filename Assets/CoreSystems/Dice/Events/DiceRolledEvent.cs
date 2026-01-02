using UnityEngine;

/// <summary>
/// Event published when dice are rolled.
/// Contains the roll result information.
/// </summary>
public class DiceRolledEvent : IEvent
{
    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public float Timestamp { get; }

    /// <summary>
    /// The result of the dice roll.
    /// </summary>
    public DiceRollResult Result { get; }

    /// <summary>
    /// Create a new DiceRolledEvent.
    /// </summary>
    /// <param name="result">The dice roll result</param>
    public DiceRolledEvent(DiceRollResult result)
    {
        if (result == null)
        {
            throw new System.ArgumentNullException(nameof(result), "Dice roll result cannot be null");
        }

        Result = result;
        Timestamp = Time.time;
    }
}

