# Catan Resource System

## Overview
The Catan Resource System wraps the generic Resource System with Catan-specific rules. It manages Catan resources (Wood, Sheep, Brick, Wheat, Ore) with standard bank supply and dice-based distribution.

**Package**: Part of `com.yourcompany.catansystems`  
**Dependencies**: Core Systems (Resource System), Infrastructure

---

## What It Does

The Catan Resource System:
- Manages Catan resource types (Wood, Sheep, Brick, Wheat, Ore)
- Initializes bank with standard supply (19 of each resource)
- Handles dice-based resource distribution
- Provides Catan-specific resource operations
- Publishes Catan-specific events

---

## How to Use

### Basic Usage

**Initialize Bank and Add Resources:**
```csharp
// Create Catan resource system (auto-registers with ServiceLocator)
CatanResourceSystem catanResources = new CatanResourceSystem();

// Initialize bank with standard supply
catanResources.InitializeBank();

// Add resources to player inventory
catanResources.AddResource(ResourceType.Wood, 5);
catanResources.AddResource(ResourceType.Sheep, 3);

// Check resources
int woodCount = catanResources.GetResourceCount(ResourceType.Wood);
bool hasEnough = catanResources.HasResource(ResourceType.Wood, 3);
```

**Draw from Bank:**
```csharp
// Draw resources from bank (supply)
bool success = catanResources.DrawFromBank(ResourceType.Wood, 3);
// Resources are automatically added to inventory
```

**Remove Resources:**
```csharp
// Remove resources (for building costs, etc.)
bool success = catanResources.RemoveResource(ResourceType.Wood, 2);
```

### Using ServiceLocator

**Get Catan Resource System:**
```csharp
// Get from ServiceLocator (must be registered first)
CatanResourceSystem catanResources = ServiceLocator.Get<CatanResourceSystem>();

if (catanResources != null)
{
    catanResources.InitializeBank();
    catanResources.AddResource(ResourceType.Wood, 5);
}
```

**Register Manually:**
```csharp
// Create without auto-registration
CatanResourceSystem catanResources = new CatanResourceSystem(autoRegister: false);

// Register manually
ServiceLocator.Register<CatanResourceSystem>(catanResources);
```

### Dice-Based Distribution

**Calculate Distribution:**
```csharp
// After dice roll (e.g., 6)
Dictionary<int, ResourceType> fieldResourceMap = GetFieldResources(); // From MapGenerator
Dictionary<int, List<int>> playerSettlements = GetPlayerSettlements(); // From BuildingSystem

Dictionary<int, Dictionary<ResourceType, int>> distribution = 
    catanResources.CalculateResourceDistribution(6, fieldResourceMap, playerSettlements);

// distribution[playerID][resourceType] = count to distribute
```

**Distribute to Player:**
```csharp
// Distribute resources to a specific player from bank
bool success = catanResources.DistributeToPlayer(
    playerID: 1, 
    ResourceType.Wood, 
    count: 1, 
    diceNumber: 6
);
// Resources are drawn from bank and added to player inventory
// CatanResourceDistributedEvent is published
```

### Listening to Events

**Subscribe to Catan Resource Events:**
```csharp
private void OnEnable()
{
    EventBus.Subscribe<CatanResourceDistributedEvent>(OnResourceDistributed);
    EventBus.Subscribe<ResourceAddedEvent>(OnResourceAdded);
    EventBus.Subscribe<ResourceRemovedEvent>(OnResourceRemoved);
}

private void OnDisable()
{
    EventBus.Unsubscribe<CatanResourceDistributedEvent>(OnResourceDistributed);
    EventBus.Unsubscribe<ResourceAddedEvent>(OnResourceAdded);
    EventBus.Unsubscribe<ResourceRemovedEvent>(OnResourceRemoved);
}

private void OnResourceDistributed(CatanResourceDistributedEvent evt)
{
    Debug.Log($"Player {evt.PlayerID} received {evt.Count} {evt.ResourceType} (dice: {evt.DiceNumber})");
    // Update UI, etc.
}

private void OnResourceAdded(ResourceAddedEvent evt)
{
    // Handle generic resource added event
}

private void OnResourceRemoved(ResourceRemovedEvent evt)
{
    // Handle generic resource removed event
}
```

---

## Catan Resource Types

- **Wood**: Used for building roads and settlements
- **Sheep**: Used for building settlements and cities
- **Brick**: Used for building roads and settlements
- **Wheat**: Used for building settlements and cities
- **Ore**: Used for building cities and development cards
- **Desert**: Not a resource (cannot be collected)

---

## Bank Supply

Standard Catan bank supply:
- **19 cards** of each resource type (Wood, Sheep, Brick, Wheat, Ore)
- Total: **95 resource cards** in bank

The bank is initialized with `InitializeBank()`.

---

## How to Modify/Extend

### Creating Custom Resource Decks

**Custom Bank Supply:**
```csharp
// Access underlying ResourceSystem
IResourceSystem resourceSystem = catanResources.GetResourceSystem();

// Create custom deck
resourceSystem.CreateResourceDeck("Wood", 30); // Custom count
```

### Extending CatanResourceSystem

**Option 1: Wrap CatanResourceSystem**
```csharp
public class EnhancedCatanResourceSystem
{
    private CatanResourceSystem catanResources;
    
    public EnhancedCatanResourceSystem()
    {
        catanResources = new CatanResourceSystem();
    }
    
    public void DistributeWithHistory(int playerID, ResourceType type, int count, int diceNumber)
    {
        catanResources.DistributeToPlayer(playerID, type, count, diceNumber);
        // Track in history
    }
}
```

**Option 2: Extend CatanResourceSystem**
```csharp
public class CustomCatanResourceSystem : CatanResourceSystem
{
    public CustomCatanResourceSystem(bool autoRegister = true) 
        : base(autoRegister) { }
    
    // Add custom methods
}
```

---

## Integration with Other Systems

### CatanDiceSystem Integration

**Listen to Dice Rolls:**
```csharp
private void OnEnable()
{
    EventBus.Subscribe<CatanDiceRolledEvent>(OnDiceRolled);
}

private void OnDiceRolled(CatanDiceRolledEvent evt)
{
    if (!evt.IsRobber)
    {
        // Calculate and distribute resources
        CatanResourceSystem catanResources = ServiceLocator.Get<CatanResourceSystem>();
        Dictionary<int, Dictionary<ResourceType, int>> distribution = 
            catanResources.CalculateResourceDistribution(
                evt.Total, 
                fieldResourceMap, 
                playerSettlements
            );
        
        // Distribute to each player
        foreach (KeyValuePair<int, Dictionary<ResourceType, int>> playerDist in distribution)
        {
            int playerID = playerDist.Key;
            foreach (KeyValuePair<ResourceType, int> resourceDist in playerDist.Value)
            {
                catanResources.DistributeToPlayer(playerID, resourceDist.Key, resourceDist.Value, evt.Total);
            }
        }
    }
}
```

### MapGenerator Integration

**Get Field Resources:**
```csharp
// From MapGenerator or TerrainSystem
Dictionary<int, ResourceType> fieldResourceMap = new Dictionary<int, ResourceType>();

// Map field positions to resource types
foreach (Field field in fields)
{
    fieldResourceMap[field.position] = field.resourceType;
}
```

---

## Best Practices

1. **Use ServiceLocator**: Register and retrieve via ServiceLocator for easy access
2. **Initialize Bank**: Always call `InitializeBank()` at game start
3. **Subscribe to Events**: Use events for decoupled resource handling
4. **Check Bank Availability**: Check `GetBankCount()` before distributing
5. **Handle Robber**: Don't distribute resources when dice roll is 7

---

## Testing

Use the `CatanResourceSystemTest` script to validate functionality:
1. Add `CatanResourceSystemTest` component to a GameObject
2. Run the scene
3. Check console for test results

All tests should pass:
- ✅ Initialize Bank
- ✅ Add Resource
- ✅ Remove Resource
- ✅ Has Resource
- ✅ Draw From Bank
- ✅ Transfer Resource
- ✅ Calculate Distribution
- ✅ Distribute To Player
- ✅ Event Publishing
- ✅ Service Locator
- ✅ Reset

---

## Catan Rules Implementation

### Resource Distribution Rules
- **Dice Roll**: Resources distributed when dice number matches field number
- **Settlements**: 1 resource per settlement
- **Cities**: 2 resources per city (to be implemented in Building System)
- **Robber**: No distribution on 7 (robber activation)
- **Desert**: No resources from desert fields

### Bank Supply Rules
- **Standard Supply**: 19 of each resource type
- **Bank Depletion**: If bank runs out, distribution stops
- **No Negative Resources**: Cannot have negative resource counts

---

## Version History

- **1.0.0** (2024-12-19): Initial release
  - Catan resource system implementation
  - Bank initialization
  - Dice-based distribution
  - Event publishing
  - ServiceLocator integration

---

## Support

For issues or questions, refer to the main project documentation or create an issue in the repository.

