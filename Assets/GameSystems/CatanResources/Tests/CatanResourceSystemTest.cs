using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Test script to validate Catan Resource System functionality.
/// Add this to a GameObject in a scene to test the Catan resource system.
/// </summary>
public class CatanResourceSystemTest : MonoBehaviour
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

    [ContextMenu("Run Catan Resource System Tests")]
    public void RunTests()
    {
        Debug.Log("=== Starting Catan Resource System Tests ===");
        Debug.Log("");

        EventBus.EnableDebugLogging = enableEventBusLogging;

        bool initializeBankTest = TestInitializeBank();
        bool addResourceTest = TestAddResource();
        bool removeResourceTest = TestRemoveResource();
        bool hasResourceTest = TestHasResource();
        bool drawFromBankTest = TestDrawFromBank();
        bool transferResourceTest = TestTransferResource();
        bool calculateDistributionTest = TestCalculateDistribution();
        bool distributeToPlayerTest = TestDistributeToPlayer();
        bool eventTest = TestEventPublishing();
        bool serviceLocatorTest = TestServiceLocator();
        bool resetTest = TestReset();

        Debug.Log("");
        Debug.Log("=== Catan Resource System Tests Summary ===");
        Debug.Log($"Initialize Bank: {(initializeBankTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Add Resource: {(addResourceTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Remove Resource: {(removeResourceTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Has Resource: {(hasResourceTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Draw From Bank: {(drawFromBankTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Transfer Resource: {(transferResourceTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Calculate Distribution: {(calculateDistributionTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Distribute To Player: {(distributeToPlayerTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Event Publishing: {(eventTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Service Locator: {(serviceLocatorTest ? "✅ PASSED" : "❌ FAILED")}");
        Debug.Log($"Reset: {(resetTest ? "✅ PASSED" : "❌ FAILED")}");

        bool allPassed = initializeBankTest && addResourceTest && removeResourceTest && hasResourceTest &&
                         drawFromBankTest && transferResourceTest && calculateDistributionTest &&
                         distributeToPlayerTest && eventTest && serviceLocatorTest && resetTest;
        Debug.Log("");
        Debug.Log($"Overall: {(allPassed ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}");
        Debug.Log("=== Catan Resource System Tests Complete ===");
    }

    private bool TestInitializeBank()
    {
        Debug.Log("--- Testing Initialize Bank ---");

        try
        {
            CatanResourceSystem catanResources = new CatanResourceSystem(autoRegister: false);
            catanResources.InitializeBank();

            int woodCount = catanResources.GetBankCount(ResourceType.Wood);
            int sheepCount = catanResources.GetBankCount(ResourceType.Sheep);
            int brickCount = catanResources.GetBankCount(ResourceType.Brick);

            if (woodCount != 19)
            {
                Debug.LogError($"❌ Initialize Bank: Expected 19 Wood, got {woodCount}");
                return false;
            }

            if (sheepCount != 19)
            {
                Debug.LogError($"❌ Initialize Bank: Expected 19 Sheep, got {sheepCount}");
                return false;
            }

            if (brickCount != 19)
            {
                Debug.LogError($"❌ Initialize Bank: Expected 19 Brick, got {brickCount}");
                return false;
            }

            Debug.Log($"✅ Initialize Bank: Bank initialized (Wood: {woodCount}, Sheep: {sheepCount}, Brick: {brickCount})");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Initialize Bank: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestAddResource()
    {
        Debug.Log("--- Testing Add Resource ---");

        try
        {
            CatanResourceSystem catanResources = new CatanResourceSystem(autoRegister: false);
            catanResources.AddResource(ResourceType.Wood, 5);

            int count = catanResources.GetResourceCount(ResourceType.Wood);

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
            CatanResourceSystem catanResources = new CatanResourceSystem(autoRegister: false);
            catanResources.AddResource(ResourceType.Wood, 5);

            bool success = catanResources.RemoveResource(ResourceType.Wood, 3);

            if (!success)
            {
                Debug.LogError("❌ Remove Resource: Removal failed");
                return false;
            }

            int count = catanResources.GetResourceCount(ResourceType.Wood);

            if (count != 2)
            {
                Debug.LogError($"❌ Remove Resource: Expected 2, got {count}");
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
            CatanResourceSystem catanResources = new CatanResourceSystem(autoRegister: false);
            catanResources.AddResource(ResourceType.Wood, 5);

            if (!catanResources.HasResource(ResourceType.Wood, 3))
            {
                Debug.LogError("❌ Has Resource: Should have 3 Wood");
                return false;
            }

            if (catanResources.HasResource(ResourceType.Wood, 10))
            {
                Debug.LogError("❌ Has Resource: Should not have 10 Wood");
                return false;
            }

            if (catanResources.HasResource(ResourceType.Sheep, 1))
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

    private bool TestDrawFromBank()
    {
        Debug.Log("--- Testing Draw From Bank ---");

        try
        {
            CatanResourceSystem catanResources = new CatanResourceSystem(autoRegister: false);
            catanResources.InitializeBank();

            int initialBankCount = catanResources.GetBankCount(ResourceType.Wood);
            bool success = catanResources.DrawFromBank(ResourceType.Wood, 3);

            if (!success)
            {
                Debug.LogError("❌ Draw From Bank: Draw failed");
                return false;
            }

            int bankCount = catanResources.GetBankCount(ResourceType.Wood);
            int inventoryCount = catanResources.GetResourceCount(ResourceType.Wood);

            if (bankCount != initialBankCount - 3)
            {
                Debug.LogError($"❌ Draw From Bank: Bank should have {initialBankCount - 3}, got {bankCount}");
                return false;
            }

            if (inventoryCount != 3)
            {
                Debug.LogError($"❌ Draw From Bank: Inventory should have 3, got {inventoryCount}");
                return false;
            }

            Debug.Log($"✅ Draw From Bank: Drew 3 Wood (bank: {bankCount}, inventory: {inventoryCount})");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Draw From Bank: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestTransferResource()
    {
        Debug.Log("--- Testing Transfer Resource ---");

        try
        {
            CatanResourceSystem source = new CatanResourceSystem(autoRegister: false);
            CatanResourceSystem target = new CatanResourceSystem(autoRegister: false);

            source.AddResource(ResourceType.Wood, 5);

            bool success = source.TransferResource(target, ResourceType.Wood, 3);

            if (!success)
            {
                Debug.LogError("❌ Transfer Resource: Transfer failed");
                return false;
            }

            int sourceCount = source.GetResourceCount(ResourceType.Wood);
            int targetCount = target.GetResourceCount(ResourceType.Wood);

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

    private bool TestCalculateDistribution()
    {
        Debug.Log("--- Testing Calculate Distribution ---");

        try
        {
            CatanResourceSystem catanResources = new CatanResourceSystem(autoRegister: false);

            // Create test data
            Dictionary<int, ResourceType> fieldResourceMap = new Dictionary<int, ResourceType>
            {
                { 0, ResourceType.Wood },
                { 1, ResourceType.Sheep },
                { 2, ResourceType.Brick }
            };

            Dictionary<int, List<int>> playerSettlements = new Dictionary<int, List<int>>
            {
                { 0, new List<int> { 1 } }, // Player 1 has settlement on field 0 (Wood)
                { 1, new List<int> { 1, 2 } } // Players 1 and 2 have settlements on field 1 (Sheep)
            };

            Dictionary<int, Dictionary<ResourceType, int>> distribution = 
                catanResources.CalculateResourceDistribution(6, fieldResourceMap, playerSettlements);

            if (distribution == null)
            {
                Debug.LogError("❌ Calculate Distribution: Result is null");
                return false;
            }

            if (!distribution.ContainsKey(1))
            {
                Debug.LogError("❌ Calculate Distribution: Player 1 not in distribution");
                return false;
            }

            if (distribution[1][ResourceType.Wood] != 1)
            {
                Debug.LogError($"❌ Calculate Distribution: Player 1 should get 1 Wood, got {distribution[1][ResourceType.Wood]}");
                return false;
            }

            if (distribution[1][ResourceType.Sheep] != 1)
            {
                Debug.LogError($"❌ Calculate Distribution: Player 1 should get 1 Sheep, got {distribution[1][ResourceType.Sheep]}");
                return false;
            }

            Debug.Log("✅ Calculate Distribution: Distribution calculated correctly");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Calculate Distribution: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestDistributeToPlayer()
    {
        Debug.Log("--- Testing Distribute To Player ---");

        try
        {
            CatanResourceSystem catanResources = new CatanResourceSystem(autoRegister: false);
            catanResources.InitializeBank();

            int initialBankCount = catanResources.GetBankCount(ResourceType.Wood);
            bool success = catanResources.DistributeToPlayer(playerID: 1, ResourceType.Wood, count: 2, diceNumber: 6);

            if (!success)
            {
                Debug.LogError("❌ Distribute To Player: Distribution failed");
                return false;
            }

            int bankCount = catanResources.GetBankCount(ResourceType.Wood);
            int inventoryCount = catanResources.GetResourceCount(ResourceType.Wood);

            if (bankCount != initialBankCount - 2)
            {
                Debug.LogError($"❌ Distribute To Player: Bank should have {initialBankCount - 2}, got {bankCount}");
                return false;
            }

            if (inventoryCount != 2)
            {
                Debug.LogError($"❌ Distribute To Player: Inventory should have 2, got {inventoryCount}");
                return false;
            }

            Debug.Log($"✅ Distribute To Player: Distributed 2 Wood to player 1 (bank: {bankCount}, inventory: {inventoryCount})");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Distribute To Player: Exception thrown: {ex.Message}");
            return false;
        }
    }

    private bool TestEventPublishing()
    {
        Debug.Log("--- Testing Event Publishing ---");

        bool resourceDistributedReceived = false;

        System.Action<CatanResourceDistributedEvent> handler = (evt) =>
        {
            resourceDistributedReceived = true;
            Debug.Log($"CatanResourceDistributedEvent received: Player {evt.PlayerID} got {evt.Count} {evt.ResourceType} (dice: {evt.DiceNumber})");
        };

        EventBus.Subscribe<CatanResourceDistributedEvent>(handler);

        try
        {
            CatanResourceSystem catanResources = new CatanResourceSystem(autoRegister: false);
            catanResources.InitializeBank();
            catanResources.DistributeToPlayer(playerID: 1, ResourceType.Wood, count: 1, diceNumber: 6);

            // Events are synchronous
            if (!resourceDistributedReceived)
            {
                Debug.LogError("❌ Event Publishing: CatanResourceDistributedEvent not received");
                EventBus.Unsubscribe<CatanResourceDistributedEvent>(handler);
                return false;
            }

            Debug.Log("✅ Event Publishing: Event received correctly");
            EventBus.Unsubscribe<CatanResourceDistributedEvent>(handler);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Event Publishing: Exception thrown: {ex.Message}");
            EventBus.Unsubscribe<CatanResourceDistributedEvent>(handler);
            return false;
        }
    }

    private bool TestServiceLocator()
    {
        Debug.Log("--- Testing Service Locator ---");

        try
        {
            CatanResourceSystem catanResources = new CatanResourceSystem(autoRegister: true);

            CatanResourceSystem retrieved = ServiceLocator.Get<CatanResourceSystem>();

            if (retrieved == null)
            {
                Debug.LogError("❌ Service Locator: Could not retrieve CatanResourceSystem");
                catanResources.Unregister();
                return false;
            }

            if (retrieved != catanResources)
            {
                Debug.LogError("❌ Service Locator: Retrieved instance doesn't match");
                catanResources.Unregister();
                return false;
            }

            Debug.Log("✅ Service Locator: CatanResourceSystem registered and retrievable");
            catanResources.Unregister();
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
            CatanResourceSystem catanResources = new CatanResourceSystem(autoRegister: false);
            catanResources.InitializeBank();
            catanResources.AddResource(ResourceType.Wood, 5);
            catanResources.DrawFromBank(ResourceType.Sheep, 3);

            catanResources.Reset();

            if (catanResources.GetResourceCount(ResourceType.Wood) != 0)
            {
                Debug.LogError("❌ Reset: Wood count should be 0");
                return false;
            }

            if (catanResources.GetBankCount(ResourceType.Sheep) != 0)
            {
                Debug.LogError("❌ Reset: Sheep bank should be empty");
                return false;
            }

            Debug.Log("✅ Reset: All resources and banks cleared");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Reset: Exception thrown: {ex.Message}");
            return false;
        }
    }
}

