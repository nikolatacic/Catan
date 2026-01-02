using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuration for creating a deck of cards.
/// Defines card types and their counts in the deck.
/// </summary>
[CreateAssetMenu(fileName = "CardConfig", menuName = "Core Systems/Card Config", order = 2)]
public class CardConfig : ScriptableObject
{
    [System.Serializable]
    public class CardTypeEntry
    {
        [Tooltip("Type identifier for the card (e.g., 'Knight', 'RoadBuilding')")]
        [SerializeField] public string cardType;

        [Tooltip("Name of the card (optional, defaults to cardType)")]
        [SerializeField] public string cardName;

        [Tooltip("Number of this card type in the deck")]
        [SerializeField] public int count = 1;
    }

    [Header("Deck Configuration")]
    [Tooltip("List of card types and their counts")]
    [SerializeField] private List<CardTypeEntry> cardTypes = new List<CardTypeEntry>();

    /// <summary>
    /// Get the list of card type entries.
    /// </summary>
    public List<CardTypeEntry> CardTypes
    {
        get => cardTypes;
        set => cardTypes = value ?? new List<CardTypeEntry>();
    }

    /// <summary>
    /// Get the total number of cards this config will create.
    /// </summary>
    public int TotalCardCount
    {
        get
        {
            int total = 0;
            if (cardTypes != null)
            {
                foreach (CardTypeEntry entry in cardTypes)
                {
                    total += entry.count;
                }
            }
            return total;
        }
    }

    private void OnValidate()
    {
        if (cardTypes == null)
        {
            cardTypes = new List<CardTypeEntry>();
        }

        // Ensure valid counts
        foreach (CardTypeEntry entry in cardTypes)
        {
            if (entry.count < 1)
            {
                entry.count = 1;
            }

            if (string.IsNullOrEmpty(entry.cardType))
            {
                entry.cardType = "Unknown";
            }

            if (string.IsNullOrEmpty(entry.cardName))
            {
                entry.cardName = entry.cardType;
            }
        }
    }
}

