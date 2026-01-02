/// <summary>
/// Interface for dice rolling systems.
/// Provides a generic contract for any dice implementation.
/// </summary>
public interface IDiceSystem
{
    /// <summary>
    /// Rolls the specified number of dice with default 6 sides.
    /// </summary>
    /// <param name="diceCount">Number of dice to roll</param>
    /// <returns>DiceRollResult containing individual die values and total</returns>
    DiceRollResult Roll(int diceCount);

    /// <summary>
    /// Rolls the specified number of dice with the given number of sides.
    /// </summary>
    /// <param name="diceCount">Number of dice to roll</param>
    /// <param name="sides">Number of sides per die</param>
    /// <returns>DiceRollResult containing individual die values and total</returns>
    DiceRollResult Roll(int diceCount, int sides);

    /// <summary>
    /// Rolls dice using the provided configuration.
    /// </summary>
    /// <param name="config">Dice configuration to use</param>
    /// <returns>DiceRollResult containing individual die values and total</returns>
    DiceRollResult Roll(DiceConfig config);

    /// <summary>
    /// Gets the last roll result.
    /// </summary>
    /// <returns>Last DiceRollResult, or null if no rolls have been made</returns>
    DiceRollResult GetLastResult();

    /// <summary>
    /// Resets the dice system, clearing the last result.
    /// </summary>
    void Reset();
}

