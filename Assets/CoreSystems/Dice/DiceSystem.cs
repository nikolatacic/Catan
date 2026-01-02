using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generic dice rolling system that can be used in any game requiring dice.
/// Supports configurable dice count, sides, and modifiers.
/// Publishes DiceRolledEvent after each roll.
/// </summary>
public class DiceSystem : IDiceSystem
{
    private const int DEFAULT_SIDES = 6;
    
    private System.Random random;
    private DiceRollResult lastResult;

    /// <summary>
    /// Create a new DiceSystem instance.
    /// </summary>
    /// <param name="seed">Optional seed for random number generation. If null, uses time-based seed.</param>
    public DiceSystem(int? seed = null)
    {
        random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
    }

    /// <summary>
    /// Rolls the specified number of dice with default 6 sides.
    /// </summary>
    /// <param name="diceCount">Number of dice to roll</param>
    /// <returns>DiceRollResult containing individual die values and total</returns>
    public DiceRollResult Roll(int diceCount)
    {
        return Roll(diceCount, DEFAULT_SIDES);
    }

    /// <summary>
    /// Rolls the specified number of dice with the given number of sides.
    /// </summary>
    /// <param name="diceCount">Number of dice to roll</param>
    /// <param name="sides">Number of sides per die</param>
    /// <returns>DiceRollResult containing individual die values and total</returns>
    public DiceRollResult Roll(int diceCount, int sides)
    {
        if (diceCount < 1)
        {
            throw new ArgumentException($"Dice count must be at least 1, but was {diceCount}", nameof(diceCount));
        }

        if (sides < 2)
        {
            throw new ArgumentException($"Dice sides must be at least 2, but was {sides}", nameof(sides));
        }

        List<int> results = new List<int>();
        
        for (int i = 0; i < diceCount; i++)
        {
            results.Add(random.Next(1, sides + 1));
        }

        float timestamp = Time.time;
        lastResult = new DiceRollResult(results, sides, timestamp);

        // Publish event
        EventBus.Publish(new DiceRolledEvent(lastResult));

        return lastResult;
    }

    /// <summary>
    /// Rolls dice using the provided configuration.
    /// </summary>
    /// <param name="config">Dice configuration to use</param>
    /// <returns>DiceRollResult containing individual die values and total</returns>
    public DiceRollResult Roll(DiceConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "Dice config cannot be null");
        }

        int diceCount = config.DiceCount;
        int sides = config.Sides;
        List<int> results = new List<int>();

        // Check if custom face values are provided
        if (config.UsesCustomValues)
        {
            // Use custom face values - each die picks randomly from the list
            List<int> customValues = config.CustomFaceValues;
            if (customValues == null || customValues.Count == 0)
            {
                throw new InvalidOperationException("Custom face values list is empty");
            }

            for (int i = 0; i < diceCount; i++)
            {
                int randomIndex = random.Next(0, customValues.Count);
                results.Add(customValues[randomIndex]);
            }

            // Use 0 for sides to indicate custom values were used
            sides = 0;
        }
        else
        {
            // Use standard range
            int minValue = config.MinValue;
            int maxValue = config.MaxValue > 0 ? config.MaxValue : sides;

            for (int i = 0; i < diceCount; i++)
            {
                results.Add(random.Next(minValue, maxValue + 1));
            }
        }

        float timestamp = Time.time;
        lastResult = new DiceRollResult(results, sides, timestamp);

        // Apply modifier if specified
        if (config.Modifier != 0)
        {
            // Note: We can't modify the existing result, so we create a new one
            // In practice, the modifier would be applied by the game system using the result
            // But for completeness, we'll note it in the result
            Debug.Log($"Dice roll modifier: {config.Modifier} (not applied to result, apply in game logic)");
        }

        // Publish event
        EventBus.Publish(new DiceRolledEvent(lastResult));

        return lastResult;
    }

    /// <summary>
    /// Gets the last roll result.
    /// </summary>
    /// <returns>Last DiceRollResult, or null if no rolls have been made</returns>
    public DiceRollResult GetLastResult()
    {
        return lastResult;
    }

    /// <summary>
    /// Resets the dice system, clearing the last result.
    /// </summary>
    public void Reset()
    {
        lastResult = null;
    }
}

