using UnityEngine;

/// <summary>
/// Catan-specific dice system that wraps the generic DiceSystem.
/// Always rolls 2 dice with 6 sides each, and handles Catan-specific rules (robber on 7).
/// </summary>
public class CatanDiceSystem
{
    private const int CATAN_DICE_COUNT = 2;
    private const int CATAN_DICE_SIDES = 6;
    private const int ROBBER_TRIGGER = 7;

    private IDiceSystem diceSystem;
    private DiceConfig catanDiceConfig;

    /// <summary>
    /// Create a new CatanDiceSystem instance.
    /// </summary>
    /// <param name="seed">Optional seed for random number generation. If null, uses time-based seed.</param>
    /// <param name="autoRegister">Whether to automatically register with ServiceLocator. Default: true.</param>
    public CatanDiceSystem(int? seed = null, bool autoRegister = true)
    {
        // Create dice system instance
        diceSystem = new DiceSystem(seed);

        // Create Catan-specific dice config (2d6)
        catanDiceConfig = ScriptableObject.CreateInstance<DiceConfig>();
        catanDiceConfig.DiceCount = CATAN_DICE_COUNT;
        catanDiceConfig.Sides = CATAN_DICE_SIDES;
        catanDiceConfig.MinValue = 1;
        catanDiceConfig.MaxValue = CATAN_DICE_SIDES;

        // Register with ServiceLocator if requested
        if (autoRegister)
        {
            ServiceLocator.Register<CatanDiceSystem>(this);
            // Also register as IDiceSystem for compatibility
            ServiceLocator.Register<IDiceSystem>(diceSystem);
        }
    }

    /// <summary>
    /// Roll Catan dice (always 2d6).
    /// </summary>
    /// <returns>CatanDiceRolledEvent with Catan-specific information</returns>
    public CatanDiceRolledEvent Roll()
    {
        // Roll using Catan config (2d6)
        DiceRollResult result = diceSystem.Roll(catanDiceConfig);

        // Create and publish Catan-specific event
        CatanDiceRolledEvent catanEvent = new CatanDiceRolledEvent(result);
        EventBus.Publish(catanEvent);

        return catanEvent;
    }

    /// <summary>
    /// Get the last roll result from the underlying dice system.
    /// </summary>
    /// <returns>Last DiceRollResult, or null if no rolls have been made</returns>
    public DiceRollResult GetLastResult()
    {
        return diceSystem.GetLastResult();
    }

    /// <summary>
    /// Reset the dice system.
    /// </summary>
    public void Reset()
    {
        diceSystem.Reset();
    }

    /// <summary>
    /// Check if a given total would trigger the robber.
    /// </summary>
    /// <param name="total">Dice total to check</param>
    /// <returns>True if total equals 7 (robber trigger)</returns>
    public static bool IsRobberTrigger(int total)
    {
        return total == ROBBER_TRIGGER;
    }

    /// <summary>
    /// Unregister from ServiceLocator.
    /// Call this when disposing of the system.
    /// </summary>
    public void Unregister()
    {
        ServiceLocator.Unregister<CatanDiceSystem>();
    }
}

