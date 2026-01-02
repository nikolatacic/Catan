using UnityEngine;

/// <summary>
/// Test script to validate Catan Dice System functionality.
/// Add this to a GameObject in a scene to test the Catan dice system.
/// </summary>
public class CatanDiceSystemTest : MonoBehaviour
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

    [ContextMenu("Run Catan Dice System Tests")]
    public void RunTests()
    {
        Debug.Log("=== Starting Catan Dice System Tests ===");
        Debug.Log("");

        EventBus.EnableDebugLogging = enableEventBusLogging;

        bool rollTest = TestRoll();
        bool robberTest = TestRobberActivation();
        bool eventTest = TestEventPublishing();
        bool serviceLocatorTest = TestServiceLocator();

        Debug.Log("");
        Debug.Log("=== Catan Dice System Tests Summary ===");
        Debug.Log($"Roll: {(rollTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Robber Activation: {(robberTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Event Publishing: {(eventTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Service Locator: {(serviceLocatorTest ? "✅ PASSED" : "❌ FAILED")}");

        bool allPassed = rollTest && robberTest && eventTest && serviceLocatorTest;
        Debug.Log("");
        Debug.Log($"Overall: {(allPassed ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}");
        Debug.Log("=== Catan Dice System Tests Complete ===");
    }

    private bool TestRoll()
    {
        Debug.Log("--- Testing Catan Dice Roll ---");

        try
        {
            // Create CatanDiceSystem
            CatanDiceSystem catanDice = new CatanDiceSystem(autoRegister: false);

            CatanDiceRolledEvent rollEvent = catanDice.Roll();

            if (rollEvent == null)
            {
                Debug.LogError("❌ Roll: Event is null");
                return false;
            }

            if (rollEvent.FirstDie < 1 || rollEvent.FirstDie > 6)
            {
                Debug.LogError($"❌ Roll: First die out of range (1-6), got {rollEvent.FirstDie}");
                return false;
            }

            if (rollEvent.SecondDie < 1 || rollEvent.SecondDie > 6)
            {
                Debug.LogError($"❌ Roll: Second die out of range (1-6), got {rollEvent.SecondDie}");
                return false;
            }

            if (rollEvent.Total < 2 || rollEvent.Total > 12)
            {
                Debug.LogError($"❌ Roll: Total out of range (2-12), got {rollEvent.Total}");
                return false;
            }

            if (rollEvent.Total != rollEvent.FirstDie + rollEvent.SecondDie)
            {
                Debug.LogError($"❌ Roll: Total doesn't match sum of dice. Total: {rollEvent.Total}, Sum: {rollEvent.FirstDie + rollEvent.SecondDie}");
                return false;
            }

            Debug.Log($"✅ Roll: Rolled {rollEvent.FirstDie} + {rollEvent.SecondDie} = {rollEvent.Total}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Roll: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestRobberActivation()
    {
        Debug.Log("--- Testing Robber Activation ---");

        try
        {
            // Test IsRobberTrigger static method
            if (!CatanDiceSystem.IsRobberTrigger(7))
            {
                Debug.LogError("❌ Robber Activation: 7 should trigger robber");
                return false;
            }

            if (CatanDiceSystem.IsRobberTrigger(6))
            {
                Debug.LogError("❌ Robber Activation: 6 should not trigger robber");
                return false;
            }

            if (CatanDiceSystem.IsRobberTrigger(8))
            {
                Debug.LogError("❌ Robber Activation: 8 should not trigger robber");
                return false;
            }

            // Test actual roll (may need multiple attempts to get a 7)
            CatanDiceSystem catanDice = new CatanDiceSystem(autoRegister: false);

            bool foundRobber = false;
            for (int i = 0; i < 100; i++)
            {
                CatanDiceRolledEvent rollEvent = catanDice.Roll();
                if (rollEvent.IsRobber)
                {
                    foundRobber = true;
                    if (rollEvent.Total != 7)
                    {
                        Debug.LogError($"❌ Robber Activation: IsRobber is true but total is {rollEvent.Total}");
                        return false;
                    }
                    Debug.Log($"✅ Robber Activation: Found robber roll on attempt {i + 1}: {rollEvent.FirstDie} + {rollEvent.SecondDie} = 7");
                    break;
                }
            }

            if (!foundRobber)
            {
                Debug.LogWarning("⚠️ Robber Activation: Did not roll a 7 in 100 attempts (statistically unlikely but possible)");
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Robber Activation: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestEventPublishing()
    {
        Debug.Log("--- Testing Event Publishing ---");

        bool eventReceived = false;
        CatanDiceRolledEvent receivedEvent = null;

        System.Action<CatanDiceRolledEvent> handler = (evt) =>
        {
            eventReceived = true;
            receivedEvent = evt;
            Debug.Log($"Event received: {evt.FirstDie} + {evt.SecondDie} = {evt.Total}, Robber: {evt.IsRobber}");
        };

        EventBus.Subscribe<CatanDiceRolledEvent>(handler);

        try
        {
            CatanDiceSystem catanDice = new CatanDiceSystem(autoRegister: false);

            CatanDiceRolledEvent rollEvent = catanDice.Roll();

            // EventBus is synchronous
            if (!eventReceived)
            {
                Debug.LogError("❌ Event Publishing: Event was not received");
                EventBus.Unsubscribe<CatanDiceRolledEvent>(handler);
                return false;
            }

            if (receivedEvent == null || receivedEvent.Total != rollEvent.Total)
            {
                Debug.LogError("❌ Event Publishing: Received event doesn't match roll event");
                EventBus.Unsubscribe<CatanDiceRolledEvent>(handler);
                return false;
            }

            Debug.Log("✅ Event Publishing: Event received correctly");
            EventBus.Unsubscribe<CatanDiceRolledEvent>(handler);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Event Publishing: Exception thrown: {ex.Message}");
            EventBus.Unsubscribe<CatanDiceRolledEvent>(handler);
            return false;
        }
    }

    private bool TestServiceLocator()
    {
        Debug.Log("--- Testing Service Locator ---");

        try
        {
            CatanDiceSystem catanDice = new CatanDiceSystem(autoRegister: true);

            CatanDiceSystem retrieved = ServiceLocator.Get<CatanDiceSystem>();

            if (retrieved == null)
            {
                Debug.LogError("❌ Service Locator: Could not retrieve CatanDiceSystem");
                catanDice.Unregister();
                return false;
            }

            if (retrieved != catanDice)
            {
                Debug.LogError("❌ Service Locator: Retrieved instance doesn't match");
                catanDice.Unregister();
                return false;
            }

            Debug.Log("✅ Service Locator: CatanDiceSystem registered and retrievable");
            catanDice.Unregister();
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Service Locator: Exception thrown: {ex.Message}");
            return false;
        }
    }
}

