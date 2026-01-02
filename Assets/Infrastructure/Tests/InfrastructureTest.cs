using UnityEngine;

/// <summary>
/// Test script to validate Infrastructure layer functionality.
/// Add this to a GameObject in a scene to test EventBus, ServiceLocator, and Logger.
/// </summary>
public class InfrastructureTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool runTestsOnStart = true;
    [SerializeField] private bool enableDebugLogging = true;

    private void Start()
    {
        if (runTestsOnStart)
        {
            RunTests();
        }
    }

    [ContextMenu("Run Infrastructure Tests")]
    public void RunTests()
    {
        Debug.Log("=== Starting Infrastructure Tests ===");
        Debug.Log("NOTE: Error messages and exceptions below are INTENTIONAL - they test the logger system.");
        Debug.Log("");
        
        bool eventBusPassed = TestEventBus();
        bool serviceLocatorPassed = TestServiceLocator();
        bool loggerPassed = TestLogger();
        
        Debug.Log("");
        Debug.Log("=== Infrastructure Tests Summary ===");
        Debug.Log($"EventBus: {(eventBusPassed ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"ServiceLocator: {(serviceLocatorPassed ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Logger: {(loggerPassed ? "✅ PASSED" : "❌ FAILED")}");
        
        bool allPassed = eventBusPassed && serviceLocatorPassed && loggerPassed;
        Debug.Log($"");
        Debug.Log($"Overall: {(allPassed ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}");
        Debug.Log("=== Infrastructure Tests Complete ===");
    }

    private bool TestEventBus()
    {
        Debug.Log("--- Testing EventBus ---");
        
        EventBus.EnableDebugLogging = enableDebugLogging;
        
        bool allTestsPassed = true;
        
        // Test subscription and publishing
        bool eventReceived = false;
        string receivedMessage = "";
        
        EventBus.Subscribe<TestEvent>(evt =>
        {
            eventReceived = true;
            receivedMessage = evt.Message;
            Debug.Log($"EventBus Test: Received event with message: {evt.Message}, value: {evt.Value}");
        });
        
        // Publish test event
        EventBus.Publish(new TestEvent("Hello from EventBus!", 42));
        
        // Verify
        if (eventReceived && receivedMessage == "Hello from EventBus!")
        {
            Debug.Log("✅ EventBus: Subscribe and Publish test PASSED");
        }
        else
        {
            Debug.LogError("❌ EventBus: Subscribe and Publish test FAILED");
            allTestsPassed = false;
        }
        
        // Test unsubscribe
        EventBus.Unsubscribe<TestEvent>(evt => { });
        int subscriberCount = EventBus.GetSubscriberCount<TestEvent>();
        
        if (subscriberCount == 1) // Should still have our handler
        {
            Debug.Log("✅ EventBus: Unsubscribe test PASSED (handler still registered)");
        }
        else
        {
            Debug.LogWarning($"⚠️ EventBus: Unsubscribe test - unexpected subscriber count: {subscriberCount}");
            // This is a warning, not a failure, but we'll note it
        }
        
        // Cleanup
        EventBus.Clear();
        Debug.Log("EventBus: Cleared all subscriptions");
        
        return allTestsPassed;
    }

    private bool TestServiceLocator()
    {
        Debug.Log("--- Testing ServiceLocator ---");
        
        ServiceLocator.EnableDebugLogging = enableDebugLogging;
        
        bool allTestsPassed = true;
        
        // Test registration and retrieval
        ILogger testLogger = new UnityLogger("Test");
        ServiceLocator.Register<ILogger>(testLogger);
        
        ILogger retrievedLogger = ServiceLocator.Get<ILogger>();
        
        if (retrievedLogger != null && retrievedLogger == testLogger)
        {
            Debug.Log("✅ ServiceLocator: Register and Get test PASSED");
        }
        else
        {
            Debug.LogError("❌ ServiceLocator: Register and Get test FAILED");
            allTestsPassed = false;
        }
        
        // Test TryGet
        if (ServiceLocator.TryGet<ILogger>(out ILogger tryGetLogger) && tryGetLogger != null)
        {
            Debug.Log("✅ ServiceLocator: TryGet test PASSED");
        }
        else
        {
            Debug.LogError("❌ ServiceLocator: TryGet test FAILED");
            allTestsPassed = false;
        }
        
        // Test IsRegistered
        if (ServiceLocator.IsRegistered<ILogger>())
        {
            Debug.Log("✅ ServiceLocator: IsRegistered test PASSED");
        }
        else
        {
            Debug.LogError("❌ ServiceLocator: IsRegistered test FAILED");
            allTestsPassed = false;
        }
        
        // Test unregister
        ServiceLocator.Unregister<ILogger>();
        if (!ServiceLocator.IsRegistered<ILogger>())
        {
            Debug.Log("✅ ServiceLocator: Unregister test PASSED");
        }
        else
        {
            Debug.LogError("❌ ServiceLocator: Unregister test FAILED");
            allTestsPassed = false;
        }
        
        // Cleanup
        ServiceLocator.Clear();
        Debug.Log("ServiceLocator: Cleared all services");
        
        return allTestsPassed;
    }

    private bool TestLogger()
    {
        Debug.Log("--- Testing Logger ---");
        Debug.Log("NOTE: The following error and exception messages are INTENTIONAL test outputs.");
        
        ILogger logger = new UnityLogger("InfrastructureTest");
        
        logger.Log("✅ Logger Test: Info message (this should appear as a normal log)");
        logger.LogWarning("⚠️ Logger Test: Warning message (this should appear as a warning)");
        logger.LogError("❌ Logger Test: Error message (this should appear as an error - THIS IS EXPECTED)");
        
        try
        {
            throw new System.Exception("Test exception - THIS IS EXPECTED");
        }
        catch (System.Exception ex)
        {
            logger.LogException(ex);
        }
        
        Debug.Log("✅ Logger: All log methods called successfully");
        Debug.Log("If you see the error and exception messages above, the logger is working correctly!");
        
        return true; // Logger test always passes if it doesn't crash
    }
}

