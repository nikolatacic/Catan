using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Data class representing the result of a dice roll.
/// Contains individual die values and the total sum.
/// </summary>
[Serializable]
public class DiceRollResult
{
    /// <summary>
    /// Individual results for each die rolled.
    /// </summary>
    public List<int> IndividualResults { get; private set; }

    /// <summary>
    /// Total sum of all dice rolled.
    /// </summary>
    public int Total { get; private set; }

    /// <summary>
    /// Number of dice that were rolled.
    /// </summary>
    public int DiceCount { get; private set; }

    /// <summary>
    /// Number of sides each die had.
    /// </summary>
    public int Sides { get; private set; }

    /// <summary>
    /// Timestamp when the roll was made (in seconds since game start).
    /// </summary>
    public float Timestamp { get; private set; }

    /// <summary>
    /// Create a new DiceRollResult.
    /// </summary>
    /// <param name="individualResults">List of individual die results</param>
    /// <param name="sides">Number of sides each die had (or 0 if custom values were used)</param>
    /// <param name="timestamp">Timestamp when roll was made</param>
    public DiceRollResult(List<int> individualResults, int sides, float timestamp)
    {
        if (individualResults == null || individualResults.Count == 0)
        {
            throw new ArgumentException("Individual results cannot be null or empty", nameof(individualResults));
        }

        IndividualResults = new List<int>(individualResults);
        DiceCount = individualResults.Count;
        Sides = sides;
        Timestamp = timestamp;

        // Calculate total
        Total = individualResults.Sum();
    }

    /// <summary>
    /// Get a string representation of the roll result.
    /// </summary>
    public override string ToString()
    {
        string sidesText = Sides > 0 ? Sides.ToString() : "custom";
        return $"DiceRollResult: {DiceCount}d{sidesText} = [{string.Join(", ", IndividualResults)}] Total: {Total}";
    }
}

