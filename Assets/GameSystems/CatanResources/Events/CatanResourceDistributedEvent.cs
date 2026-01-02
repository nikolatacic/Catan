using UnityEngine;

/// <summary>
/// Event published when resources are distributed to players based on dice roll.
/// </summary>
public class CatanResourceDistributedEvent : IEvent
{
    /// <summary>
    /// Timestamp when the event was created.
    /// </summary>
    public float Timestamp { get; }

    /// <summary>
    /// ID of the player who received resources.
    /// </summary>
    public int PlayerID { get; }

    /// <summary>
    /// Type of resource distributed.
    /// </summary>
    public ResourceType ResourceType { get; }

    /// <summary>
    /// Number of resources distributed.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// The dice number that triggered this distribution.
    /// </summary>
    public int DiceNumber { get; }

    /// <summary>
    /// Create a new CatanResourceDistributedEvent.
    /// </summary>
    /// <param name="playerID">ID of the player who received resources</param>
    /// <param name="resourceType">Type of resource distributed</param>
    /// <param name="count">Number of resources distributed</param>
    /// <param name="diceNumber">The dice number that triggered distribution</param>
    public CatanResourceDistributedEvent(int playerID, ResourceType resourceType, int count, int diceNumber)
    {
        PlayerID = playerID;
        ResourceType = resourceType;
        Count = count;
        DiceNumber = diceNumber;
        Timestamp = Time.time;
    }
}

