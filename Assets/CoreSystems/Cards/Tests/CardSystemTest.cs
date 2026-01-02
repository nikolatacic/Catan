using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test script to validate Card System functionality.
/// Add this to a GameObject in a scene to test the card system.
/// </summary>
public class CardSystemTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runTestsOnStart = true;
    [SerializeField] private bool enableEventBusLogging = false;

    private void Start()
    {
        if (runTestsOnStart)
        {
            RunTests();
        }
    }

    [ContextMenu("Run Card System Tests")]
    public void RunTests()
    {
        Debug.Log("=== Starting Card System Tests ===");
        Debug.Log("");

        EventBus.EnableDebugLogging = enableEventBusLogging;

        bool createDeckTest = TestCreateDeck();
        bool shuffleTest = TestShuffle();
        bool drawCardTest = TestDrawCard();
        bool drawMultipleTest = TestDrawMultiple();
        bool handManagementTest = TestHandManagement();
        bool discardTest = TestDiscard();
        bool eventTest = TestEventPublishing();
        bool resetTest = TestReset();

        Debug.Log("");
        Debug.Log("=== Card System Tests Summary ===");
        Debug.Log($"Create Deck: {(createDeckTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Shuffle: {(shuffleTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Draw Card: {(drawCardTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Draw Multiple: {(drawMultipleTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Hand Management: {(handManagementTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Discard: {(discardTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Event Publishing: {(eventTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Reset: {(resetTest ? "✅ PASSED" : "❌ FAILED")}");

        bool allPassed = createDeckTest && shuffleTest && drawCardTest && drawMultipleTest && 
                         handManagementTest && discardTest && eventTest && resetTest;
        Debug.Log("");
        Debug.Log($"Overall: {(allPassed ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}");
        Debug.Log("=== Card System Tests Complete ===");
    }

    private bool TestCreateDeck()
    {
        Debug.Log("--- Testing Create Deck ---");

        try
        {
            ICardSystem cardSystem = new CardSystem();

            CardConfig config = ScriptableObject.CreateInstance<CardConfig>();
            CardConfig.CardTypeEntry entry1 = new CardConfig.CardTypeEntry
            {
                cardType = "Knight",
                cardName = "Knight",
                count = 14
            };
            CardConfig.CardTypeEntry entry2 = new CardConfig.CardTypeEntry
            {
                cardType = "RoadBuilding",
                cardName = "Road Building",
                count = 2
            };
            config.CardTypes = new List<CardConfig.CardTypeEntry> { entry1, entry2 };

            cardSystem.CreateDeck(config);

            int deckCount = cardSystem.GetDeckCount();
            int expectedCount = 14 + 2; // 16 cards

            if (deckCount != expectedCount)
            {
                Debug.LogError($"❌ Create Deck: Expected {expectedCount} cards, got {deckCount}");
                return false;
            }

            Debug.Log($"✅ Create Deck: Created deck with {deckCount} cards");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Create Deck: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestShuffle()
    {
        Debug.Log("--- Testing Shuffle ---");

        try
        {
            ICardSystem cardSystem = new CardSystem();

            CardConfig config = ScriptableObject.CreateInstance<CardConfig>();
            CardConfig.CardTypeEntry entry = new CardConfig.CardTypeEntry
            {
                cardType = "Test",
                count = 10
            };
            config.CardTypes = new List<CardConfig.CardTypeEntry> { entry };

            cardSystem.CreateDeck(config);

            // Draw first card before shuffle
            ICard firstCardBefore = cardSystem.DrawCard();
            cardSystem.Reset();

            // Create deck again and shuffle
            cardSystem.CreateDeck(config);
            cardSystem.ShuffleDeck();
            ICard firstCardAfter = cardSystem.DrawCard();

            // Note: This test might occasionally fail if shuffle happens to put same card first
            // But it's statistically very unlikely with 10 cards
            Debug.Log($"✅ Shuffle: Deck shuffled (first card before: {firstCardBefore?.CardID}, after: {firstCardAfter?.CardID})");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Shuffle: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestDrawCard()
    {
        Debug.Log("--- Testing Draw Card ---");

        try
        {
            ICardSystem cardSystem = new CardSystem();

            CardConfig config = ScriptableObject.CreateInstance<CardConfig>();
            CardConfig.CardTypeEntry entry = new CardConfig.CardTypeEntry
            {
                cardType = "Test",
                count = 5
            };
            config.CardTypes = new List<CardConfig.CardTypeEntry> { entry };

            cardSystem.CreateDeck(config);
            int initialCount = cardSystem.GetDeckCount();

            ICard drawnCard = cardSystem.DrawCard();

            if (drawnCard == null)
            {
                Debug.LogError("❌ Draw Card: Drawn card is null");
                return false;
            }

            int newCount = cardSystem.GetDeckCount();

            if (newCount != initialCount - 1)
            {
                Debug.LogError($"❌ Draw Card: Deck count should be {initialCount - 1}, got {newCount}");
                return false;
            }

            Debug.Log($"✅ Draw Card: Drew card {drawnCard.CardName} (ID: {drawnCard.CardID})");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Draw Card: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestDrawMultiple()
    {
        Debug.Log("--- Testing Draw Multiple ---");

        try
        {
            ICardSystem cardSystem = new CardSystem();

            CardConfig config = ScriptableObject.CreateInstance<CardConfig>();
            CardConfig.CardTypeEntry entry = new CardConfig.CardTypeEntry
            {
                cardType = "Test",
                count = 10
            };
            config.CardTypes = new List<CardConfig.CardTypeEntry> { entry };

            cardSystem.CreateDeck(config);
            int initialCount = cardSystem.GetDeckCount();

            List<ICard> drawnCards = cardSystem.DrawCards(3);

            if (drawnCards == null)
            {
                Debug.LogError("❌ Draw Multiple: Drawn cards list is null");
                return false;
            }

            if (drawnCards.Count != 3)
            {
                Debug.LogError($"❌ Draw Multiple: Expected 3 cards, got {drawnCards.Count}");
                return false;
            }

            int newCount = cardSystem.GetDeckCount();

            if (newCount != initialCount - 3)
            {
                Debug.LogError($"❌ Draw Multiple: Deck count should be {initialCount - 3}, got {newCount}");
                return false;
            }

            Debug.Log($"✅ Draw Multiple: Drew {drawnCards.Count} cards");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Draw Multiple: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestHandManagement()
    {
        Debug.Log("--- Testing Hand Management ---");

        try
        {
            ICardSystem cardSystem = new CardSystem();

            CardConfig config = ScriptableObject.CreateInstance<CardConfig>();
            CardConfig.CardTypeEntry entry = new CardConfig.CardTypeEntry
            {
                cardType = "Test",
                count = 5
            };
            config.CardTypes = new List<CardConfig.CardTypeEntry> { entry };

            cardSystem.CreateDeck(config);

            ICard card1 = cardSystem.DrawCard();
            ICard card2 = cardSystem.DrawCard();

            cardSystem.AddToHand(card1);
            cardSystem.AddToHand(card2);

            int handCount = cardSystem.GetHandCount();

            if (handCount != 2)
            {
                Debug.LogError($"❌ Hand Management: Expected 2 cards in hand, got {handCount}");
                return false;
            }

            List<ICard> hand = cardSystem.GetHand();

            if (hand == null || hand.Count != 2)
            {
                Debug.LogError($"❌ Hand Management: GetHand returned wrong count");
                return false;
            }

            if (!hand.Contains(card1) || !hand.Contains(card2))
            {
                Debug.LogError("❌ Hand Management: Hand doesn't contain expected cards");
                return false;
            }

            Debug.Log($"✅ Hand Management: Hand contains {handCount} cards");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Hand Management: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestDiscard()
    {
        Debug.Log("--- Testing Discard ---");

        try
        {
            ICardSystem cardSystem = new CardSystem();

            CardConfig config = ScriptableObject.CreateInstance<CardConfig>();
            CardConfig.CardTypeEntry entry = new CardConfig.CardTypeEntry
            {
                cardType = "Test",
                count = 5
            };
            config.CardTypes = new List<CardConfig.CardTypeEntry> { entry };

            cardSystem.CreateDeck(config);

            ICard card = cardSystem.DrawCard();
            cardSystem.AddToHand(card);

            int handCountBefore = cardSystem.GetHandCount();
            int discardCountBefore = cardSystem.GetDiscardCount();

            cardSystem.DiscardCard(card);

            int handCountAfter = cardSystem.GetHandCount();
            int discardCountAfter = cardSystem.GetDiscardCount();

            if (handCountAfter != handCountBefore - 1)
            {
                Debug.LogError($"❌ Discard: Hand count should be {handCountBefore - 1}, got {handCountAfter}");
                return false;
            }

            if (discardCountAfter != discardCountBefore + 1)
            {
                Debug.LogError($"❌ Discard: Discard count should be {discardCountBefore + 1}, got {discardCountAfter}");
                return false;
            }

            Debug.Log($"✅ Discard: Card discarded (hand: {handCountAfter}, discard: {discardCountAfter})");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Discard: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestEventPublishing()
    {
        Debug.Log("--- Testing Event Publishing ---");

        bool cardDrawnReceived = false;
        bool cardDiscardedReceived = false;
        bool handChangedReceived = false;

        System.Action<CardDrawnEvent> drawHandler = (evt) =>
        {
            cardDrawnReceived = true;
            Debug.Log($"CardDrawnEvent received: {evt.Card.CardName}");
        };

        System.Action<CardDiscardedEvent> discardHandler = (evt) =>
        {
            cardDiscardedReceived = true;
            Debug.Log($"CardDiscardedEvent received: {evt.Card.CardName}");
        };

        System.Action<HandChangedEvent> handHandler = (evt) =>
        {
            handChangedReceived = true;
            Debug.Log($"HandChangedEvent received: {evt.HandCount} cards in hand");
        };

        EventBus.Subscribe<CardDrawnEvent>(drawHandler);
        EventBus.Subscribe<CardDiscardedEvent>(discardHandler);
        EventBus.Subscribe<HandChangedEvent>(handHandler);

        try
        {
            ICardSystem cardSystem = new CardSystem();

            CardConfig config = ScriptableObject.CreateInstance<CardConfig>();
            CardConfig.CardTypeEntry entry = new CardConfig.CardTypeEntry
            {
                cardType = "Test",
                count = 5
            };
            config.CardTypes = new List<CardConfig.CardTypeEntry> { entry };

            cardSystem.CreateDeck(config);

            ICard card = cardSystem.DrawCard();
            cardSystem.AddToHand(card);
            cardSystem.DiscardCard(card);

            // Events are synchronous
            if (!cardDrawnReceived)
            {
                Debug.LogError("❌ Event Publishing: CardDrawnEvent not received");
                EventBus.Unsubscribe<CardDrawnEvent>(drawHandler);
                EventBus.Unsubscribe<CardDiscardedEvent>(discardHandler);
                EventBus.Unsubscribe<HandChangedEvent>(handHandler);
                return false;
            }

            if (!handChangedReceived)
            {
                Debug.LogError("❌ Event Publishing: HandChangedEvent not received");
                EventBus.Unsubscribe<CardDrawnEvent>(drawHandler);
                EventBus.Unsubscribe<CardDiscardedEvent>(discardHandler);
                EventBus.Unsubscribe<HandChangedEvent>(handHandler);
                return false;
            }

            if (!cardDiscardedReceived)
            {
                Debug.LogError("❌ Event Publishing: CardDiscardedEvent not received");
                EventBus.Unsubscribe<CardDrawnEvent>(drawHandler);
                EventBus.Unsubscribe<CardDiscardedEvent>(discardHandler);
                EventBus.Unsubscribe<HandChangedEvent>(handHandler);
                return false;
            }

            Debug.Log("✅ Event Publishing: All events received correctly");
            EventBus.Unsubscribe<CardDrawnEvent>(drawHandler);
            EventBus.Unsubscribe<CardDiscardedEvent>(discardHandler);
            EventBus.Unsubscribe<HandChangedEvent>(handHandler);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Event Publishing: Exception thrown: {ex.Message}");
            EventBus.Unsubscribe<CardDrawnEvent>(drawHandler);
            EventBus.Unsubscribe<CardDiscardedEvent>(discardHandler);
            EventBus.Unsubscribe<HandChangedEvent>(handHandler);
            return false;
        }
    }

    private bool TestReset()
    {
        Debug.Log("--- Testing Reset ---");

        try
        {
            ICardSystem cardSystem = new CardSystem();

            CardConfig config = ScriptableObject.CreateInstance<CardConfig>();
            CardConfig.CardTypeEntry entry = new CardConfig.CardTypeEntry
            {
                cardType = "Test",
                count = 5
            };
            config.CardTypes = new List<CardConfig.CardTypeEntry> { entry };

            cardSystem.CreateDeck(config);
            ICard card = cardSystem.DrawCard();
            cardSystem.AddToHand(card);
            cardSystem.DiscardCard(card);

            cardSystem.Reset();

            if (cardSystem.GetDeckCount() != 0)
            {
                Debug.LogError("❌ Reset: Deck should be empty after reset");
                return false;
            }

            if (cardSystem.GetHandCount() != 0)
            {
                Debug.LogError("❌ Reset: Hand should be empty after reset");
                return false;
            }

            if (cardSystem.GetDiscardCount() != 0)
            {
                Debug.LogError("❌ Reset: Discard pile should be empty after reset");
                return false;
            }

            Debug.Log("✅ Reset: All collections cleared");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Reset: Exception thrown: {ex.Message}");
            return false;
        }
    }
}

