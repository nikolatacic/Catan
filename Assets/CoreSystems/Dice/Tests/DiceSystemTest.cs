using UnityEngine;

/// <summary>
/// Test script to validate Dice System functionality.
/// Add this to a GameObject in a scene to test the dice system.
/// </summary>
public class DiceSystemTest : MonoBehaviour
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

    [ContextMenu("Run Dice System Tests")]
    public void RunTests()
    {
        Debug.Log("=== Starting Dice System Tests ===");
        Debug.Log("");

        EventBus.EnableDebugLogging = enableEventBusLogging;

        bool basicRollTest = TestBasicRoll();
        bool customSidesTest = TestCustomSides();
        bool configTest = TestConfigRoll();
        bool customValuesTest = TestCustomValues();
        bool eventTest = TestEventPublishing();
        bool lastResultTest = TestLastResult();
        bool resetTest = TestReset();

        Debug.Log("");
        Debug.Log("=== Dice System Tests Summary ===");
        Debug.Log($"Basic Roll: {(basicRollTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Custom Sides: {(customSidesTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Config Roll: {(configTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Custom Values: {(customValuesTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Event Publishing: {(eventTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Last Result: {(lastResultTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Reset: {(resetTest ? "✅ PASSED" : "❌ FAILED")}");

        bool allPassed = basicRollTest && customSidesTest && configTest && customValuesTest && eventTest && lastResultTest && resetTest;
        Debug.Log("");
        Debug.Log($"Overall: {(allPassed ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}");
        Debug.Log("=== Dice System Tests Complete ===");
    }

    private bool TestCustomValues()
    {
        Debug.Log("--- Testing Custom Face Values ---");

        try
        {
            DiceConfig config = ScriptableObject.CreateInstance<DiceConfig>();
            config.DiceCount = 1;
            config.CustomFaceValues = new System.Collections.Generic.List<int> { 1, 1, 4 };

            IDiceSystem diceSystem = new DiceSystem();
            DiceRollResult result = diceSystem.Roll(config);

            if (result == null)
            {
                Debug.LogError("❌ Custom Values: Result is null");
                return false;
            }

            if (result.DiceCount != 1)
            {
                Debug.LogError($"❌ Custom Values: Expected 1 die, got {result.DiceCount}");
                return false;
            }

            // Result should be one of the custom values
            if (!config.CustomFaceValues.Contains(result.IndividualResults[0]))
            {
                Debug.LogError($"❌ Custom Values: Result {result.IndividualResults[0]} not in custom values {string.Join(", ", config.CustomFaceValues)}");
                return false;
            }

            Debug.Log($"✅ Custom Values: Rolled {result.IndividualResults[0]} (from custom values: {string.Join(", ", config.CustomFaceValues)})");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Custom Values: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestBasicRoll()
    {
        Debug.Log("--- Testing Basic Roll (2 dice, 6 sides) ---");

        try
        {
            IDiceSystem diceSystem = new DiceSystem();
            DiceRollResult result = diceSystem.Roll(2);

            if (result == null)
            {
                Debug.LogError("❌ Basic Roll: Result is null");
                return false;
            }

            if (result.DiceCount != 2)
            {
                Debug.LogError($"❌ Basic Roll: Expected 2 dice, got {result.DiceCount}");
                return false;
            }

            if (result.Sides != 6)
            {
                Debug.LogError($"❌ Basic Roll: Expected 6 sides, got {result.Sides}");
                return false;
            }

            if (result.IndividualResults == null || result.IndividualResults.Count != 2)
            {
                Debug.LogError("❌ Basic Roll: Individual results invalid");
                return false;
            }

            if (result.Total < 2 || result.Total > 12)
            {
                Debug.LogError($"❌ Basic Roll: Total out of range (2-12), got {result.Total}");
                return false;
            }

            Debug.Log($"✅ Basic Roll: Rolled {result.DiceCount}d{result.Sides} = {result.Total} [{string.Join(", ", result.IndividualResults)}]");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Basic Roll: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestCustomSides()
    {
        Debug.Log("--- Testing Custom Sides (1 die, 20 sides) ---");

        try
        {
            IDiceSystem diceSystem = new DiceSystem();
            DiceRollResult result = diceSystem.Roll(1, 20);

            if (result == null)
            {
                Debug.LogError("❌ Custom Sides: Result is null");
                return false;
            }

            if (result.DiceCount != 1)
            {
                Debug.LogError($"❌ Custom Sides: Expected 1 die, got {result.DiceCount}");
                return false;
            }

            if (result.Sides != 20)
            {
                Debug.LogError($"❌ Custom Sides: Expected 20 sides, got {result.Sides}");
                return false;
            }

            if (result.Total < 1 || result.Total > 20)
            {
                Debug.LogError($"❌ Custom Sides: Total out of range (1-20), got {result.Total}");
                return false;
            }

            Debug.Log($"✅ Custom Sides: Rolled {result.DiceCount}d{result.Sides} = {result.Total}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Custom Sides: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestConfigRoll()
    {
        Debug.Log("--- Testing Config Roll ---");

        try
        {
            DiceConfig config = ScriptableObject.CreateInstance<DiceConfig>();
            config.DiceCount = 3;
            config.Sides = 6;
            config.MinValue = 1;
            config.MaxValue = 6;

            IDiceSystem diceSystem = new DiceSystem();
            DiceRollResult result = diceSystem.Roll(config);

            if (result == null)
            {
                Debug.LogError("❌ Config Roll: Result is null");
                return false;
            }

            if (result.DiceCount != 3)
            {
                Debug.LogError($"❌ Config Roll: Expected 3 dice, got {result.DiceCount}");
                return false;
            }

            Debug.Log($"✅ Config Roll: Rolled {result.DiceCount}d{result.Sides} = {result.Total} [{string.Join(", ", result.IndividualResults)}]");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Config Roll: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestEventPublishing()
    {
        Debug.Log("--- Testing Event Publishing ---");

        bool eventReceived = false;
        DiceRollResult receivedResult = null;

        System.Action<DiceRolledEvent> handler = (evt) =>
        {
            eventReceived = true;
            receivedResult = evt.Result;
            Debug.Log($"Event received: {evt.Result}");
        };

        EventBus.Subscribe<DiceRolledEvent>(handler);

        try
        {
            IDiceSystem diceSystem = new DiceSystem();
            DiceRollResult result = diceSystem.Roll(2, 6);

            // EventBus is synchronous, so event should be received immediately
            if (!eventReceived)
            {
                Debug.LogError("❌ Event Publishing: Event was not received");
                EventBus.Unsubscribe<DiceRolledEvent>(handler);
                return false;
            }

            if (receivedResult == null || receivedResult.Total != result.Total)
            {
                Debug.LogError("❌ Event Publishing: Event result doesn't match roll result");
                EventBus.Unsubscribe<DiceRolledEvent>(handler);
                return false;
            }

            Debug.Log("✅ Event Publishing: Event received correctly");
            EventBus.Unsubscribe<DiceRolledEvent>(handler);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Event Publishing: Exception thrown: {ex.Message}");
            EventBus.Unsubscribe<DiceRolledEvent>(handler);
            return false;
        }
    }

    private bool TestLastResult()
    {
        Debug.Log("--- Testing Last Result ---");

        try
        {
            IDiceSystem diceSystem = new DiceSystem();
            
            // No roll yet, should be null
            if (diceSystem.GetLastResult() != null)
            {
                Debug.LogError("❌ Last Result: Expected null before first roll");
                return false;
            }

            DiceRollResult firstRoll = diceSystem.Roll(2, 6);
            DiceRollResult lastResult = diceSystem.GetLastResult();

            if (lastResult == null)
            {
                Debug.LogError("❌ Last Result: Expected result after roll, got null");
                return false;
            }

            if (lastResult.Total != firstRoll.Total)
            {
                Debug.LogError("❌ Last Result: Last result doesn't match roll result");
                return false;
            }

            DiceRollResult secondRoll = diceSystem.Roll(1, 20);
            lastResult = diceSystem.GetLastResult();

            if (lastResult.Total != secondRoll.Total)
            {
                Debug.LogError("❌ Last Result: Last result doesn't match second roll");
                return false;
            }

            Debug.Log("✅ Last Result: Last result tracking works correctly");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Last Result: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestReset()
    {
        Debug.Log("--- Testing Reset ---");

        try
        {
            IDiceSystem diceSystem = new DiceSystem();
            diceSystem.Roll(2, 6);

            if (diceSystem.GetLastResult() == null)
            {
                Debug.LogError("❌ Reset: Expected result before reset");
                return false;
            }

            diceSystem.Reset();

            if (diceSystem.GetLastResult() != null)
            {
                Debug.LogError("❌ Reset: Expected null after reset");
                return false;
            }

            Debug.Log("✅ Reset: Reset works correctly");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Reset: Exception thrown: {ex.Message}");
            return false;
        }
    }
}

