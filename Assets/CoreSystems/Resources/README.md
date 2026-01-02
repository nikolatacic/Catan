# Resource System Package

## Overview
The Resource System provides a generic, reusable resource/inventory management system using a hybrid approach. It uses CardSystem internally for card-level tracking while providing a simple interface for resource operations.

**Package Name**: `com.yourcompany.coresystems` (Resources module)  
**Version**: 1.0.0  
**Dependencies**: Infrastructure package, Card System package

---

## What It Does

The Resource System allows you to:
- Add and remove resources from inventories
- Check resource availability
- Transfer resources between inventories
- Create resource decks (for bank/supply)
- Draw resources from decks
- Track resources at the card level (for future mods/customization)

---

## Hybrid Approach

This system uses **CardSystem internally** (one CardSystem per resource type) while providing a **simple resource interface**. This gives you:
- **Simple API**: Easy resource operations (add, remove, check)
- **Card-level tracking**: Individual cards tracked for future customization
- **Flexibility**: Can access CardSystem directly for advanced operations
- **Reusability**: Uses existing CardSystem infrastructure

---

## How to Use

### Basic Usage

**Add and Remove Resources:**
```csharp
// Create resource system
IResourceSystem resourceSystem = new ResourceSystem();

// Add resources
resourceSystem.AddResource("Wood", 5);
resourceSystem.AddResource("Sheep", 3);

// Check resources
int woodCount = resourceSystem.GetResourceCount("Wood");
bool hasEnough = resourceSystem.HasResource("Wood", 3);

// Remove resources
bool success = resourceSystem.RemoveResource("Wood", 2);
```

**Create Resource Decks (Bank/Supply):**
```csharp
// Create a deck of 18 Wood cards (for bank)
resourceSystem.CreateResourceDeck("Wood", 18);
resourceSystem.CreateResourceDeck("Sheep", 18);
resourceSystem.CreateResourceDeck("Brick", 18);
resourceSystem.CreateResourceDeck("Wheat", 18);
resourceSystem.CreateResourceDeck("Ore", 18);

// Draw from bank
resourceSystem.DrawFromDeck("Wood", 3); // Draws 3 Wood into inventory
```

**Transfer Resources:**
```csharp
IResourceSystem player1 = new ResourceSystem();
IResourceSystem player2 = new ResourceSystem();

player1.AddResource("Wood", 5);

// Transfer 3 Wood from player1 to player2
player1.TransferResource(player2, "Wood", 3);
```

### Listening to Events

**Subscribe to Resource Events:**
```csharp
private void OnEnable()
{
    EventBus.Subscribe<ResourceAddedEvent>(OnResourceAdded);
    EventBus.Subscribe<ResourceRemovedEvent>(OnResourceRemoved);
    EventBus.Subscribe<ResourceChangedEvent>(OnResourceChanged);
}

private void OnDisable()
{
    EventBus.Unsubscribe<ResourceAddedEvent>(OnResourceAdded);
    EventBus.Unsubscribe<ResourceRemovedEvent>(OnResourceRemoved);
    EventBus.Unsubscribe<ResourceChangedEvent>(OnResourceChanged);
}

private void OnResourceAdded(ResourceAddedEvent evt)
{
    Debug.Log($"Added {evt.Count} {evt.ResourceType}");
    // Update UI, etc.
}

private void OnResourceRemoved(ResourceRemovedEvent evt)
{
    Debug.Log($"Removed {evt.Count} {evt.ResourceType}");
}

private void OnResourceChanged(ResourceChangedEvent evt)
{
    Debug.Log($"{evt.ResourceType} count is now {evt.NewCount}");
    // Update UI display
}
```

### Using ServiceLocator

**Register Resource System:**
```csharp
// In initialization
IResourceSystem resourceSystem = new ResourceSystem();
ServiceLocator.Register<IResourceSystem>(resourceSystem);
```

**Get Resource System:**
```csharp
// Anywhere in your code
IResourceSystem resourceSystem = ServiceLocator.Get<IResourceSystem>();
resourceSystem.AddResource("Wood", 5);
```

### Advanced: Access CardSystem Directly

**For Custom Operations:**
```csharp
ResourceSystem resourceSystem = new ResourceSystem();
ResourceInventory inventory = resourceSystem.GetInventory();

// Access CardSystem for a specific resource type
ICardSystem woodCardSystem = inventory.GetInventoryCardSystem("Wood");
if (woodCardSystem != null)
{
    List<ICard> woodCards = woodCardSystem.GetHand();
    // Do something with individual cards
}
```

---

## How to Modify/Upgrade/Extend

### Creating Custom Resource Types

**Using String Types:**
```csharp
// Works with any string identifier
resourceSystem.AddResource("Gold", 10);
resourceSystem.AddResource("Crystal", 5);
resourceSystem.AddResource("CustomResource", 100);
```

**Using Enums (in Catan-specific layer):**
```csharp
// In CatanResourceSystem
public enum CatanResourceType
{
    Wood, Sheep, Brick, Wheat, Ore
}

// Convert enum to string
resourceSystem.AddResource(CatanResourceType.Wood.ToString(), 5);
```

### Extending ResourceSystem

**Option 1: Wrap ResourceSystem**
```csharp
public class EnhancedResourceSystem
{
    private IResourceSystem resourceSystem;
    
    public EnhancedResourceSystem()
    {
        resourceSystem = new ResourceSystem();
    }
    
    public void AddResourceWithHistory(string type, int count)
    {
        resourceSystem.AddResource(type, count);
        // Track in history
    }
}
```

**Option 2: Implement IResourceSystem**
```csharp
public class CustomResourceSystem : IResourceSystem
{
    // Implement all interface methods
    public void AddResource(string resourceType, int count) { /* Custom logic */ }
    // ... etc
}
```

### Custom Decks

**Create Custom Resource Decks:**
```csharp
// Create a deck with custom count
resourceSystem.CreateResourceDeck("SpecialResource", 50);

// Draw from custom deck
resourceSystem.DrawFromDeck("SpecialResource", 10);
```

---

## Best Practices

1. **Use Interface**: Always use `IResourceSystem` interface, not `ResourceSystem` class directly
2. **Register with ServiceLocator**: Register resource system for easy access
3. **Subscribe to Events**: Use events for decoupled resource handling
4. **Validate Before Operations**: Check `HasResource()` before removing
5. **Use Decks for Bank**: Create decks for bank/supply, use inventory for players

---

## Testing

Use the `ResourceSystemTest` script to validate functionality:
1. Add `ResourceSystemTest` component to a GameObject
2. Run the scene
3. Check console for test results

All tests should pass:
- ✅ Add Resource
- ✅ Remove Resource
- ✅ Has Resource
- ✅ Get Resource Count
- ✅ Get All Resources
- ✅ Transfer Resource
- ✅ Create Deck
- ✅ Draw From Deck
- ✅ Event Publishing
- ✅ Reset

---

## Package Extraction

When extracting to a package:
1. Copy `CoreSystems/Resources/` folder to package structure
2. Create `package.json` with Infrastructure and Card System dependencies
3. Update namespaces if needed
4. Test package in isolation
5. Ensure Infrastructure and Card System packages are available

---

## Version History

- **1.0.0** (2024-12-19): Initial release
  - Generic resource system
  - Hybrid approach with CardSystem
  - Deck management
  - Event publishing
  - ServiceLocator integration

---

## Support

For issues or questions, refer to the main project documentation or create an issue in the repository.

