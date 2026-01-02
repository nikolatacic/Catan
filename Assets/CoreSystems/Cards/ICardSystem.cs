using System.Collections.Generic;

/// <summary>
/// Interface for card system operations.
/// Provides a generic contract for deck, hand, and discard pile management.
/// </summary>
public interface ICardSystem
{
    /// <summary>
    /// Create a deck from the provided configuration.
    /// </summary>
    /// <param name="config">Card configuration to use</param>
    void CreateDeck(CardConfig config);

    /// <summary>
    /// Shuffle the deck.
    /// </summary>
    void ShuffleDeck();

    /// <summary>
    /// Draw a single card from the deck.
    /// </summary>
    /// <returns>Drawn card, or null if deck is empty</returns>
    ICard DrawCard();

    /// <summary>
    /// Draw multiple cards from the deck.
    /// </summary>
    /// <param name="count">Number of cards to draw</param>
    /// <returns>List of drawn cards (may be less than count if deck is empty)</returns>
    List<ICard> DrawCards(int count);

    /// <summary>
    /// Add a card to the hand.
    /// </summary>
    /// <param name="card">Card to add to hand</param>
    void AddToHand(ICard card);

    /// <summary>
    /// Discard a card from the hand.
    /// </summary>
    /// <param name="card">Card to discard</param>
    void DiscardCard(ICard card);

    /// <summary>
    /// Get all cards currently in hand.
    /// </summary>
    /// <returns>List of cards in hand</returns>
    List<ICard> GetHand();

    /// <summary>
    /// Get the number of cards remaining in the deck.
    /// </summary>
    /// <returns>Number of cards in deck</returns>
    int GetDeckCount();

    /// <summary>
    /// Get the number of cards in hand.
    /// </summary>
    /// <returns>Number of cards in hand</returns>
    int GetHandCount();

    /// <summary>
    /// Get the number of cards in discard pile.
    /// </summary>
    /// <returns>Number of cards in discard pile</returns>
    int GetDiscardCount();

    /// <summary>
    /// Reset the card system, clearing deck, hand, and discard.
    /// </summary>
    void Reset();
}

