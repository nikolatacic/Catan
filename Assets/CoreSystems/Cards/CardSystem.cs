using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generic card system for deck management, hand management, and card operations.
/// Can be used in any game requiring cards.
/// </summary>
public class CardSystem : ICardSystem
{
    private List<ICard> deck;
    private List<ICard> hand;
    private List<ICard> discardPile;

    /// <summary>
    /// Create a new CardSystem instance.
    /// </summary>
    public CardSystem()
    {
        deck = new List<ICard>();
        hand = new List<ICard>();
        discardPile = new List<ICard>();
    }

    /// <summary>
    /// Create a deck from the provided configuration.
    /// </summary>
    /// <param name="config">Card configuration to use</param>
    public void CreateDeck(CardConfig config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "Card config cannot be null");
        }

        if (config.CardTypes == null || config.CardTypes.Count == 0)
        {
            throw new ArgumentException("Card config must have at least one card type", nameof(config));
        }

        deck.Clear();
        hand.Clear();
        discardPile.Clear();

        foreach (CardConfig.CardTypeEntry entry in config.CardTypes)
        {
            for (int i = 0; i < entry.count; i++)
            {
                CardData card = new CardData(entry.cardType, entry.cardName);
                deck.Add(card);
            }
        }

        Debug.Log($"CardSystem: Created deck with {deck.Count} cards");
    }

    /// <summary>
    /// Shuffle the deck using Fisher-Yates algorithm.
    /// </summary>
    public void ShuffleDeck()
    {
        if (deck == null || deck.Count <= 1)
        {
            return;
        }

        System.Random random = new System.Random();
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            ICard temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }

        Debug.Log("CardSystem: Deck shuffled");
    }

    /// <summary>
    /// Draw a single card from the deck.
    /// </summary>
    /// <returns>Drawn card, or null if deck is empty</returns>
    public ICard DrawCard()
    {
        if (deck == null || deck.Count == 0)
        {
            Debug.LogWarning("CardSystem: Cannot draw card, deck is empty");
            return null;
        }

        ICard card = deck[0];
        deck.RemoveAt(0);

        EventBus.Publish(new CardDrawnEvent(card));

        return card;
    }

    /// <summary>
    /// Draw multiple cards from the deck.
    /// </summary>
    /// <param name="count">Number of cards to draw</param>
    /// <returns>List of drawn cards (may be less than count if deck is empty)</returns>
    public List<ICard> DrawCards(int count)
    {
        if (count < 1)
        {
            throw new ArgumentException($"Count must be at least 1, but was {count}", nameof(count));
        }

        List<ICard> drawnCards = new List<ICard>();

        for (int i = 0; i < count && deck.Count > 0; i++)
        {
            ICard card = DrawCard();
            if (card != null)
            {
                drawnCards.Add(card);
            }
        }

        return drawnCards;
    }

    /// <summary>
    /// Add a card to the hand.
    /// </summary>
    /// <param name="card">Card to add to hand</param>
    public void AddToHand(ICard card)
    {
        if (card == null)
        {
            throw new ArgumentNullException(nameof(card), "Card cannot be null");
        }

        if (hand == null)
        {
            hand = new List<ICard>();
        }

        hand.Add(card);
        EventBus.Publish(new HandChangedEvent(hand));
    }

    /// <summary>
    /// Discard a card from the hand.
    /// </summary>
    /// <param name="card">Card to discard</param>
    public void DiscardCard(ICard card)
    {
        if (card == null)
        {
            throw new ArgumentNullException(nameof(card), "Card cannot be null");
        }

        if (hand == null || !hand.Contains(card))
        {
            Debug.LogWarning($"CardSystem: Cannot discard card {card}, not in hand");
            return;
        }

        hand.Remove(card);

        if (discardPile == null)
        {
            discardPile = new List<ICard>();
        }

        discardPile.Add(card);

        EventBus.Publish(new CardDiscardedEvent(card));
        EventBus.Publish(new HandChangedEvent(hand));
    }

    /// <summary>
    /// Get all cards currently in hand.
    /// </summary>
    /// <returns>List of cards in hand (new list, safe to modify)</returns>
    public List<ICard> GetHand()
    {
        if (hand == null)
        {
            return new List<ICard>();
        }

        return new List<ICard>(hand);
    }

    /// <summary>
    /// Get the number of cards remaining in the deck.
    /// </summary>
    /// <returns>Number of cards in deck</returns>
    public int GetDeckCount()
    {
        return deck?.Count ?? 0;
    }

    /// <summary>
    /// Get the number of cards in hand.
    /// </summary>
    /// <returns>Number of cards in hand</returns>
    public int GetHandCount()
    {
        return hand?.Count ?? 0;
    }

    /// <summary>
    /// Get the number of cards in discard pile.
    /// </summary>
    /// <returns>Number of cards in discard pile</returns>
    public int GetDiscardCount()
    {
        return discardPile?.Count ?? 0;
    }

    /// <summary>
    /// Reset the card system, clearing deck, hand, and discard.
    /// </summary>
    public void Reset()
    {
        deck?.Clear();
        hand?.Clear();
        discardPile?.Clear();
    }
}

