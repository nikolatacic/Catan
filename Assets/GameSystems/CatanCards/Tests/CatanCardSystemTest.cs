using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test script to validate Catan Card System functionality.
/// Add this to a GameObject in a scene to test the Catan card system.
/// </summary>
public class CatanCardSystemTest : MonoBehaviour
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

    [ContextMenu("Run Catan Card System Tests")]
    public void RunTests()
    {
        Debug.Log("=== Starting Catan Card System Tests ===");
        Debug.Log("");

        EventBus.EnableDebugLogging = enableEventBusLogging;

        bool createDeckTest = TestCreateDeck();
        bool drawCardTest = TestDrawCard();
        bool handManagementTest = TestHandManagement();
        bool useCardTest = TestUseCard();
        bool eventTest = TestEventPublishing();
        bool serviceLocatorTest = TestServiceLocator();
        bool resetTest = TestReset();

        Debug.Log("");
        Debug.Log("=== Catan Card System Tests Summary ===");
        Debug.Log($"Create Deck: {(createDeckTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Draw Card: {(drawCardTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Hand Management: {(handManagementTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Use Card: {(useCardTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Event Publishing: {(eventTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Service Locator: {(serviceLocatorTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Reset: {(resetTest ? "✅ PASSED" : "❌ FAILED")}");

        bool allPassed = createDeckTest && drawCardTest && handManagementTest && 
                         useCardTest && eventTest && serviceLocatorTest && resetTest;
        Debug.Log("");
        Debug.Log($"Overall: {(allPassed ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}");
        Debug.Log("=== Catan Card System Tests Complete ===");
    }

    private bool TestCreateDeck()
    {
        Debug.Log("--- Testing Create Deck ---");

        try
        {
            CatanCardSystem catanCardSystem = new CatanCardSystem(autoRegister: false);
            catanCardSystem.CreateDevelopmentCardDeck();

            int deckCount = catanCardSystem.GetDeckCount();
            int expectedCount = 14 + 2 + 2 + 2 + 5; // 25 cards total

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

    private bool TestDrawCard()
    {
        Debug.Log("--- Testing Draw Card ---");

        try
        {
            CatanCardSystem catanCardSystem = new CatanCardSystem(autoRegister: false);
            catanCardSystem.CreateDevelopmentCardDeck();

            int initialCount = catanCardSystem.GetDeckCount();
            CatanCardDrawnEvent drawEvent = catanCardSystem.DrawDevelopmentCard();

            if (drawEvent == null)
            {
                Debug.LogError("❌ Draw Card: Draw event is null");
                return false;
            }

            if (drawEvent.Card == null)
            {
                Debug.LogError("❌ Draw Card: Card in event is null");
                return false;
            }

            int newCount = catanCardSystem.GetDeckCount();

            if (newCount != initialCount - 1)
            {
                Debug.LogError($"❌ Draw Card: Deck count should be {initialCount - 1}, got {newCount}");
                return false;
            }

            Debug.Log($"✅ Draw Card: Drew {drawEvent.Card.CardName} (Type: {drawEvent.Card.CatanType})");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Draw Card: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestHandManagement()
    {
        Debug.Log("--- Testing Hand Management ---");

        try
        {
            CatanCardSystem catanCardSystem = new CatanCardSystem(autoRegister: false);
            catanCardSystem.CreateDevelopmentCardDeck();

            CatanCardDrawnEvent drawEvent1 = catanCardSystem.DrawDevelopmentCard();
            CatanCardDrawnEvent drawEvent2 = catanCardSystem.DrawDevelopmentCard();

            catanCardSystem.AddToHand(drawEvent1.Card);
            catanCardSystem.AddToHand(drawEvent2.Card);

            int handCount = catanCardSystem.GetHandCount();

            if (handCount != 2)
            {
                Debug.LogError($"❌ Hand Management: Expected 2 cards in hand, got {handCount}");
                return false;
            }

            List<CatanCardData> hand = catanCardSystem.GetHand();

            if (hand == null || hand.Count != 2)
            {
                Debug.LogError($"❌ Hand Management: GetHand returned wrong count");
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

    private bool TestUseCard()
    {
        Debug.Log("--- Testing Use Card ---");

        try
        {
            CatanCardSystem catanCardSystem = new CatanCardSystem(autoRegister: false);
            catanCardSystem.CreateDevelopmentCardDeck();

            CatanCardDrawnEvent drawEvent = catanCardSystem.DrawDevelopmentCard();
            catanCardSystem.AddToHand(drawEvent.Card);

            int handCountBefore = catanCardSystem.GetHandCount();
            int discardCountBefore = catanCardSystem.GetDiscardCount();

            catanCardSystem.UseCard(drawEvent.Card, playerID: 1);

            int handCountAfter = catanCardSystem.GetHandCount();
            int discardCountAfter = catanCardSystem.GetDiscardCount();

            if (handCountAfter != handCountBefore - 1)
            {
                Debug.LogError($"❌ Use Card: Hand count should be {handCountBefore - 1}, got {handCountAfter}");
                return false;
            }

            if (discardCountAfter != discardCountBefore + 1)
            {
                Debug.LogError($"❌ Use Card: Discard count should be {discardCountBefore + 1}, got {discardCountAfter}");
                return false;
            }

            Debug.Log($"✅ Use Card: Card used successfully (hand: {handCountAfter}, discard: {discardCountAfter})");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Use Card: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestEventPublishing()
    {
        Debug.Log("--- Testing Event Publishing ---");

        bool cardDrawnReceived = false;
        bool cardUsedReceived = false;

        System.Action<CatanCardDrawnEvent> drawHandler = (evt) =>
        {
            cardDrawnReceived = true;
            Debug.Log($"CatanCardDrawnEvent received: {evt.Card.CardName}");
        };

        System.Action<CatanCardUsedEvent> usedHandler = (evt) =>
        {
            cardUsedReceived = true;
            Debug.Log($"CatanCardUsedEvent received: {evt.Card.CardName} by player {evt.PlayerID}");
        };

        EventBus.Subscribe<CatanCardDrawnEvent>(drawHandler);
        EventBus.Subscribe<CatanCardUsedEvent>(usedHandler);

        try
        {
            CatanCardSystem catanCardSystem = new CatanCardSystem(autoRegister: false);
            catanCardSystem.CreateDevelopmentCardDeck();

            CatanCardDrawnEvent drawEvent = catanCardSystem.DrawDevelopmentCard();
            catanCardSystem.AddToHand(drawEvent.Card);
            catanCardSystem.UseCard(drawEvent.Card, playerID: 1);

            // Events are synchronous
            if (!cardDrawnReceived)
            {
                Debug.LogError("❌ Event Publishing: CatanCardDrawnEvent not received");
                EventBus.Unsubscribe<CatanCardDrawnEvent>(drawHandler);
                EventBus.Unsubscribe<CatanCardUsedEvent>(usedHandler);
                return false;
            }

            if (!cardUsedReceived)
            {
                Debug.LogError("❌ Event Publishing: CatanCardUsedEvent not received");
                EventBus.Unsubscribe<CatanCardDrawnEvent>(drawHandler);
                EventBus.Unsubscribe<CatanCardUsedEvent>(usedHandler);
                return false;
            }

            Debug.Log("✅ Event Publishing: All events received correctly");
            EventBus.Unsubscribe<CatanCardDrawnEvent>(drawHandler);
            EventBus.Unsubscribe<CatanCardUsedEvent>(usedHandler);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Event Publishing: Exception thrown: {ex.Message}");
            EventBus.Unsubscribe<CatanCardDrawnEvent>(drawHandler);
            EventBus.Unsubscribe<CatanCardUsedEvent>(usedHandler);
            return false;
        }
    }

    private bool TestServiceLocator()
    {
        Debug.Log("--- Testing Service Locator ---");

        try
        {
            CatanCardSystem catanCardSystem = new CatanCardSystem(autoRegister: true);

            CatanCardSystem retrieved = ServiceLocator.Get<CatanCardSystem>();

            if (retrieved == null)
            {
                Debug.LogError("❌ Service Locator: Could not retrieve CatanCardSystem");
                catanCardSystem.Unregister();
                return false;
            }

            if (retrieved != catanCardSystem)
            {
                Debug.LogError("❌ Service Locator: Retrieved instance doesn't match");
                catanCardSystem.Unregister();
                return false;
            }

            Debug.Log("✅ Service Locator: CatanCardSystem registered and retrievable");
            catanCardSystem.Unregister();
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Service Locator: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestReset()
    {
        Debug.Log("--- Testing Reset ---");

        try
        {
            CatanCardSystem catanCardSystem = new CatanCardSystem(autoRegister: false);
            catanCardSystem.CreateDevelopmentCardDeck();
            catanCardSystem.DrawDevelopmentCard();
            catanCardSystem.DrawDevelopmentCard();

            catanCardSystem.Reset();

            if (catanCardSystem.GetDeckCount() != 0)
            {
                Debug.LogError("❌ Reset: Deck should be empty after reset");
                return false;
            }

            if (catanCardSystem.GetHandCount() != 0)
            {
                Debug.LogError("❌ Reset: Hand should be empty after reset");
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

