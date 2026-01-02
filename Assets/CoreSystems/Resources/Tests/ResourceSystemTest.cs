using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test script to validate Resource System functionality.
/// Add this to a GameObject in a scene to test the resource system.
/// </summary>
public class ResourceSystemTest : MonoBehaviour
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

    [ContextMenu("Run Resource System Tests")]
    public void RunTests()
    {
        Debug.Log("=== Starting Resource System Tests ===");
        Debug.Log("");

        EventBus.EnableDebugLogging = enableEventBusLogging;

        bool addResourceTest = TestAddResource();
        bool removeResourceTest = TestRemoveResource();
        bool hasResourceTest = TestHasResource();
        bool getResourceCountTest = TestGetResourceCount();
        bool getAllResourcesTest = TestGetAllResources();
        bool transferResourceTest = TestTransferResource();
        bool createDeckTest = TestCreateDeck();
        bool drawFromDeckTest = TestDrawFromDeck();
        bool eventTest = TestEventPublishing();
        bool resetTest = TestReset();

        Debug.Log("");
        Debug.Log("=== Resource System Tests Summary ===");
        Debug.Log($"Add Resource: {(addResourceTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Remove Resource: {(removeResourceTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Has Resource: {(hasResourceTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Get Resource Count: {(getResourceCountTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Get All Resources: {(getAllResourcesTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Transfer Resource: {(transferResourceTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Create Deck: {(createDeckTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Draw From Deck: {(drawFromDeckTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Event Publishing: {(eventTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Reset: {(resetTest ? "✅ PASSED" : "❌ FAILED")}");

        bool allPassed = addResourceTest && removeResourceTest && hasResourceTest && 
                         getResourceCountTest && getAllResourcesTest && transferResourceTest &&
                         createDeckTest && drawFromDeckTest && eventTest && resetTest;
        Debug.Log("");
        Debug.Log($"Overall: {(allPassed ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}");
        Debug.Log("=== Resource System Tests Complete ===");
    }

    private bool TestAddResource()
    {
        Debug.Log("--- Testing Add Resource ---");

        try
        {
            IResourceSystem resourceSystem = new ResourceSystem();
            resourceSystem.AddResource("Wood", 5);

            int count = resourceSystem.GetResourceCount("Wood");

            if (count != 5)
            {
                Debug.LogError($"❌ Add Resource: Expected 5, got {count}");
                return false;
            }

            Debug.Log($"✅ Add Resource: Added 5 Wood, count is {count}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Add Resource: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestRemoveResource()
    {
        Debug.Log("--- Testing Remove Resource ---");

        try
        {
            IResourceSystem resourceSystem = new ResourceSystem();
            resourceSystem.AddResource("Wood", 5);

            bool success = resourceSystem.RemoveResource("Wood", 3);

            if (!success)
            {
                Debug.LogError("❌ Remove Resource: Removal failed");
                return false;
            }

            int count = resourceSystem.GetResourceCount("Wood");

            if (count != 2)
            {
                Debug.LogError($"❌ Remove Resource: Expected 2, got {count}");
                return false;
            }

            // Test insufficient resources
            bool failTest = resourceSystem.RemoveResource("Wood", 5);
            if (failTest)
            {
                Debug.LogError("❌ Remove Resource: Should have failed with insufficient resources");
                return false;
            }

            Debug.Log($"✅ Remove Resource: Removed 3 Wood, count is {count}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Remove Resource: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestHasResource()
    {
        Debug.Log("--- Testing Has Resource ---");

        try
        {
            IResourceSystem resourceSystem = new ResourceSystem();
            resourceSystem.AddResource("Wood", 5);

            if (!resourceSystem.HasResource("Wood", 3))
            {
                Debug.LogError("❌ Has Resource: Should have 3 Wood");
                return false;
            }

            if (resourceSystem.HasResource("Wood", 10))
            {
                Debug.LogError("❌ Has Resource: Should not have 10 Wood");
                return false;
            }

            if (resourceSystem.HasResource("Sheep", 1))
            {
                Debug.LogError("❌ Has Resource: Should not have Sheep");
                return false;
            }

            Debug.Log("✅ Has Resource: Checks working correctly");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Has Resource: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestGetResourceCount()
    {
        Debug.Log("--- Testing Get Resource Count ---");

        try
        {
            IResourceSystem resourceSystem = new ResourceSystem();
            resourceSystem.AddResource("Wood", 5);
            resourceSystem.AddResource("Sheep", 3);

            int woodCount = resourceSystem.GetResourceCount("Wood");
            int sheepCount = resourceSystem.GetResourceCount("Sheep");
            int brickCount = resourceSystem.GetResourceCount("Brick");

            if (woodCount != 5)
            {
                Debug.LogError($"❌ Get Resource Count: Expected 5 Wood, got {woodCount}");
                return false;
            }

            if (sheepCount != 3)
            {
                Debug.LogError($"❌ Get Resource Count: Expected 3 Sheep, got {sheepCount}");
                return false;
            }

            if (brickCount != 0)
            {
                Debug.LogError($"❌ Get Resource Count: Expected 0 Brick, got {brickCount}");
                return false;
            }

            Debug.Log($"✅ Get Resource Count: Wood={woodCount}, Sheep={sheepCount}, Brick={brickCount}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Get Resource Count: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestGetAllResources()
    {
        Debug.Log("--- Testing Get All Resources ---");

        try
        {
            IResourceSystem resourceSystem = new ResourceSystem();
            resourceSystem.AddResource("Wood", 5);
            resourceSystem.AddResource("Sheep", 3);
            resourceSystem.AddResource("Brick", 2);

            Dictionary<string, int> allResources = resourceSystem.GetAllResources();

            if (allResources == null)
            {
                Debug.LogError("❌ Get All Resources: Result is null");
                return false;
            }

            if (allResources.Count != 3)
            {
                Debug.LogError($"❌ Get All Resources: Expected 3 types, got {allResources.Count}");
                return false;
            }

            if (allResources["Wood"] != 5 || allResources["Sheep"] != 3 || allResources["Brick"] != 2)
            {
                Debug.LogError($"❌ Get All Resources: Counts don't match");
                return false;
            }

            Debug.Log($"✅ Get All Resources: Retrieved {allResources.Count} resource types");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Get All Resources: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestTransferResource()
    {
        Debug.Log("--- Testing Transfer Resource ---");

        try
        {
            IResourceSystem source = new ResourceSystem();
            IResourceSystem target = new ResourceSystem();

            source.AddResource("Wood", 5);

            bool success = source.TransferResource(target, "Wood", 3);

            if (!success)
            {
                Debug.LogError("❌ Transfer Resource: Transfer failed");
                return false;
            }

            int sourceCount = source.GetResourceCount("Wood");
            int targetCount = target.GetResourceCount("Wood");

            if (sourceCount != 2)
            {
                Debug.LogError($"❌ Transfer Resource: Source should have 2, got {sourceCount}");
                return false;
            }

            if (targetCount != 3)
            {
                Debug.LogError($"❌ Transfer Resource: Target should have 3, got {targetCount}");
                return false;
            }

            Debug.Log($"✅ Transfer Resource: Transferred 3 Wood (source: {sourceCount}, target: {targetCount})");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Transfer Resource: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestCreateDeck()
    {
        Debug.Log("--- Testing Create Deck ---");

        try
        {
            IResourceSystem resourceSystem = new ResourceSystem();
            resourceSystem.CreateResourceDeck("Wood", 18);

            int deckCount = resourceSystem.GetDeckCount("Wood");

            if (deckCount != 18)
            {
                Debug.LogError($"❌ Create Deck: Expected 18 cards, got {deckCount}");
                return false;
            }

            Debug.Log($"✅ Create Deck: Created deck with {deckCount} Wood cards");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Create Deck: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestDrawFromDeck()
    {
        Debug.Log("--- Testing Draw From Deck ---");

        try
        {
            IResourceSystem resourceSystem = new ResourceSystem();
            resourceSystem.CreateResourceDeck("Wood", 18);

            int initialDeckCount = resourceSystem.GetDeckCount("Wood");
            bool success = resourceSystem.DrawFromDeck("Wood", 5);

            if (!success)
            {
                Debug.LogError("❌ Draw From Deck: Draw failed");
                return false;
            }

            int deckCount = resourceSystem.GetDeckCount("Wood");
            int inventoryCount = resourceSystem.GetResourceCount("Wood");

            if (deckCount != initialDeckCount - 5)
            {
                Debug.LogError($"❌ Draw From Deck: Deck should have {initialDeckCount - 5}, got {deckCount}");
                return false;
            }

            if (inventoryCount != 5)
            {
                Debug.LogError($"❌ Draw From Deck: Inventory should have 5, got {inventoryCount}");
                return false;
            }

            Debug.Log($"✅ Draw From Deck: Drew 5 Wood (deck: {deckCount}, inventory: {inventoryCount})");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Draw From Deck: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestEventPublishing()
    {
        Debug.Log("--- Testing Event Publishing ---");

        bool resourceAddedReceived = false;
        bool resourceRemovedReceived = false;
        bool resourceChangedReceived = false;

        System.Action<ResourceAddedEvent> addHandler = (evt) =>
        {
            resourceAddedReceived = true;
            Debug.Log($"ResourceAddedEvent received: {evt.Count} {evt.ResourceType}");
        };

        System.Action<ResourceRemovedEvent> removeHandler = (evt) =>
        {
            resourceRemovedReceived = true;
            Debug.Log($"ResourceRemovedEvent received: {evt.Count} {evt.ResourceType}");
        };

        System.Action<ResourceChangedEvent> changeHandler = (evt) =>
        {
            resourceChangedReceived = true;
            Debug.Log($"ResourceChangedEvent received: {evt.ResourceType} = {evt.NewCount}");
        };

        EventBus.Subscribe<ResourceAddedEvent>(addHandler);
        EventBus.Subscribe<ResourceRemovedEvent>(removeHandler);
        EventBus.Subscribe<ResourceChangedEvent>(changeHandler);

        try
        {
            IResourceSystem resourceSystem = new ResourceSystem();
            resourceSystem.AddResource("Wood", 5);
            resourceSystem.RemoveResource("Wood", 2);

            // Events are synchronous
            if (!resourceAddedReceived)
            {
                Debug.LogError("❌ Event Publishing: ResourceAddedEvent not received");
                EventBus.Unsubscribe<ResourceAddedEvent>(addHandler);
                EventBus.Unsubscribe<ResourceRemovedEvent>(removeHandler);
                EventBus.Unsubscribe<ResourceChangedEvent>(changeHandler);
                return false;
            }

            if (!resourceRemovedReceived)
            {
                Debug.LogError("❌ Event Publishing: ResourceRemovedEvent not received");
                EventBus.Unsubscribe<ResourceAddedEvent>(addHandler);
                EventBus.Unsubscribe<ResourceRemovedEvent>(removeHandler);
                EventBus.Unsubscribe<ResourceChangedEvent>(changeHandler);
                return false;
            }

            if (!resourceChangedReceived)
            {
                Debug.LogError("❌ Event Publishing: ResourceChangedEvent not received");
                EventBus.Unsubscribe<ResourceAddedEvent>(addHandler);
                EventBus.Unsubscribe<ResourceRemovedEvent>(removeHandler);
                EventBus.Unsubscribe<ResourceChangedEvent>(changeHandler);
                return false;
            }

            Debug.Log("✅ Event Publishing: All events received correctly");
            EventBus.Unsubscribe<ResourceAddedEvent>(addHandler);
            EventBus.Unsubscribe<ResourceRemovedEvent>(removeHandler);
            EventBus.Unsubscribe<ResourceChangedEvent>(changeHandler);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Event Publishing: Exception thrown: {ex.Message}");
            EventBus.Unsubscribe<ResourceAddedEvent>(addHandler);
            EventBus.Unsubscribe<ResourceRemovedEvent>(removeHandler);
            EventBus.Unsubscribe<ResourceChangedEvent>(changeHandler);
            return false;
        }
    }

    private bool TestReset()
    {
        Debug.Log("--- Testing Reset ---");

        try
        {
            IResourceSystem resourceSystem = new ResourceSystem();
            resourceSystem.AddResource("Wood", 5);
            resourceSystem.CreateResourceDeck("Sheep", 18);
            resourceSystem.DrawFromDeck("Sheep", 3);

            resourceSystem.Reset();

            if (resourceSystem.GetResourceCount("Wood") != 0)
            {
                Debug.LogError("❌ Reset: Wood count should be 0");
                return false;
            }

            if (resourceSystem.GetDeckCount("Sheep") != 0)
            {
                Debug.LogError("❌ Reset: Sheep deck should be empty");
                return false;
            }

            Debug.Log("✅ Reset: All resources and decks cleared");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Reset: Exception thrown: {ex.Message}");
            return false;
        }
    }
}

