using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuration for dice rolling.
/// Can be created as a ScriptableObject asset for easy configuration.
/// Supports both standard dice (1-6) and custom dice with specific face values.
/// </summary>
[CreateAssetMenu(fileName = "DiceConfig", menuName = "Core Systems/Dice Config", order = 1)]
public class DiceConfig : ScriptableObject
{
    [Header("Dice Settings")]
    [Tooltip("Number of dice to roll")]
    [SerializeField] private int diceCount = 2;

    [Tooltip("Number of sides per die (used if custom values are not set)")]
    [SerializeField] private int sides = 6;

    [Header("Custom Face Values (Optional)")]
    [Tooltip("If set, these values will be used instead of standard 1-sides range. Each die will randomly pick one value from this list.")]
    [SerializeField] private List<int> customFaceValues = new List<int>();

    [Header("Modifiers (Optional)")]
    [Tooltip("Value to add to the total result")]
    [SerializeField] private int modifier = 0;

    [Tooltip("Minimum value for each die (overrides normal range, ignored if custom values are set)")]
    [SerializeField] private int minValue = 1;

    [Tooltip("Maximum value for each die (overrides normal range, ignored if custom values are set)")]
    [SerializeField] private int maxValue = 0; // 0 means use sides value

    /// <summary>
    /// Number of dice to roll.
    /// </summary>
    public int DiceCount
    {
        get => diceCount;
        set => diceCount = Mathf.Max(1, value);
    }

    /// <summary>
    /// Number of sides per die.
    /// </summary>
    public int Sides
    {
        get => sides;
        set => sides = Mathf.Max(2, value);
    }

    /// <summary>
    /// Modifier to add to the total result.
    /// </summary>
    public int Modifier
    {
        get => modifier;
        set => modifier = value;
    }

    /// <summary>
    /// Minimum value for each die.
    /// </summary>
    public int MinValue
    {
        get => minValue;
        set => minValue = Mathf.Max(1, value);
    }

    /// <summary>
    /// Maximum value for each die.
    /// </summary>
    public int MaxValue
    {
        get => maxValue == 0 ? sides : maxValue;
        set => maxValue = value == 0 ? sides : Mathf.Max(minValue, value);
    }

    /// <summary>
    /// Custom face values for the die. If set and not empty, these values will be used instead of standard range.
    /// Each die will randomly pick one value from this list.
    /// </summary>
    public List<int> CustomFaceValues
    {
        get => customFaceValues;
        set => customFaceValues = value ?? new List<int>();
    }

    /// <summary>
    /// Whether custom face values are being used.
    /// </summary>
    public bool UsesCustomValues => customFaceValues != null && customFaceValues.Count > 0;

    private void OnValidate()
    {
        // Ensure valid values
        diceCount = Mathf.Max(1, diceCount);
        sides = Mathf.Max(2, sides);
        minValue = Mathf.Max(1, minValue);
        
        if (maxValue == 0)
        {
            maxValue = sides;
        }
        else
        {
            maxValue = Mathf.Max(minValue, maxValue);
        }

        // Ensure minValue doesn't exceed maxValue
        if (minValue > maxValue)
        {
            minValue = maxValue;
        }

        // Ensure custom face values list is initialized
        if (customFaceValues == null)
        {
            customFaceValues = new List<int>();
        }

        // Remove any invalid values from custom list
        if (customFaceValues.Count > 0)
        {
            customFaceValues.RemoveAll(v => v < 1);
        }
    }
}

