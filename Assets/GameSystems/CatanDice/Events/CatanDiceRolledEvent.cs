using UnityEngine;

/// <summary>
/// Event published when Catan dice are rolled.
/// Contains Catan-specific information including robber activation.
/// </summary>
public class CatanDiceRolledEvent : IEvent
{
    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public float Timestamp { get; }

    /// <summary>
    /// Value of the first die (1-6).
    /// </summary>
    public int FirstDie { get; }

    /// <summary>
    /// Value of the second die (1-6).
    /// </summary>
    public int SecondDie { get; }

    /// <summary>
    /// Total sum of both dice (2-12).
    /// </summary>
    public int Total { get; }

    /// <summary>
    /// Whether the roll activated the robber (total = 7).
    /// </summary>
    public bool IsRobber { get; }

    /// <summary>
    /// The underlying generic dice roll result.
    /// </summary>
    public DiceRollResult BaseResult { get; }

    /// <summary>
    /// Create a new CatanDiceRolledEvent.
    /// </summary>
    /// <param name="baseResult">The generic dice roll result (must be 2d6)</param>
    public CatanDiceRolledEvent(DiceRollResult baseResult)
    {
        if (baseResult == null)
        {
            throw new System.ArgumentNullException(nameof(baseResult), "Base result cannot be null");
        }

        if (baseResult.DiceCount != 2)
        {
            throw new System.ArgumentException("Catan dice must roll exactly 2 dice", nameof(baseResult));
        }

        if (baseResult.IndividualResults.Count != 2)
        {
            throw new System.ArgumentException("Catan dice must have exactly 2 individual results", nameof(baseResult));
        }

        BaseResult = baseResult;
        FirstDie = baseResult.IndividualResults[0];
        SecondDie = baseResult.IndividualResults[1];
        Total = baseResult.Total;
        IsRobber = Total == 7;
        Timestamp = Time.time;
    }
}

