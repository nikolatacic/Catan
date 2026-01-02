# Catan Dice System

## Overview
The Catan Dice System wraps the generic DiceSystem with Catan-specific rules. It always rolls 2 dice with 6 sides each and handles the special case where a total of 7 activates the robber.

**Package**: Part of `com.yourcompany.catansystems`  
**Dependencies**: Core Systems (Dice System), Infrastructure

---

## What It Does

The Catan Dice System:
- Always rolls exactly 2 dice with 6 sides each (2d6)
- Detects when the total is 7 (robber activation)
- Publishes Catan-specific events with detailed information
- Provides easy access via ServiceLocator

---

## How to Use

### Basic Usage

**Create and Roll:**
```csharp
// Create Catan dice system (auto-registers with ServiceLocator)
CatanDiceSystem catanDice = new CatanDiceSystem();

// Roll dice
CatanDiceRolledEvent rollEvent = catanDice.Roll();

// Check results
Debug.Log($"Rolled: {rollEvent.FirstDie} + {rollEvent.SecondDie} = {rollEvent.Total}");

// Check for robber
if (rollEvent.IsRobber)
{
    Debug.Log("Robber activated!");
    // Handle robber mechanics
}
```

### Using ServiceLocator

**Get Catan Dice System:**
```csharp
// Get from ServiceLocator (must be registered first)
CatanDiceSystem catanDice = ServiceLocator.Get<CatanDiceSystem>();

if (catanDice != null)
{
    CatanDiceRolledEvent rollEvent = catanDice.Roll();
}
```

**Register Manually:**
```csharp
// Create without auto-registration
CatanDiceSystem catanDice = new CatanDiceSystem(autoRegister: false);

// Register manually
ServiceLocator.Register<CatanDiceSystem>(catanDice);
```

### Listening to Events

**Subscribe to Catan Dice Rolls:**
```csharp
private void OnEnable()
{
    EventBus.Subscribe<CatanDiceRolledEvent>(OnCatanDiceRolled);
}

private void OnDisable()
{
    EventBus.Unsubscribe<CatanDiceRolledEvent>(OnCatanDiceRolled);
}

private void OnCatanDiceRolled(CatanDiceRolledEvent evt)
{
    Debug.Log($"Catan dice rolled: {evt.FirstDie} + {evt.SecondDie} = {evt.Total}");
    
    if (evt.IsRobber)
    {
        // Activate robber
        ActivateRobber();
    }
    else
    {
        // Distribute resources based on evt.Total
        DistributeResources(evt.Total);
    }
}
```

### Checking Robber Trigger

**Static Method:**
```csharp
// Check if a total would trigger robber
if (CatanDiceSystem.IsRobberTrigger(7))
{
    // Robber activated
}
```

---

## CatanDiceRolledEvent

The event contains:
- `FirstDie`: Value of first die (1-6)
- `SecondDie`: Value of second die (1-6)
- `Total`: Sum of both dice (2-12)
- `IsRobber`: True if total equals 7
- `BaseResult`: The underlying generic DiceRollResult
- `Timestamp`: When the roll occurred

---

## How to Modify/Extend

### Creating Custom Catan Dice Variants

**Option 1: Extend CatanDiceSystem**
```csharp
public class CustomCatanDiceSystem : CatanDiceSystem
{
    public CustomCatanDiceSystem(int? seed = null, bool autoRegister = true) 
        : base(seed, autoRegister) { }
    
    // Add custom methods
    public CatanDiceRolledEvent RollWithBonus()
    {
        CatanDiceRolledEvent roll = Roll();
        // Apply bonus logic
        return roll;
    }
}
```

**Option 2: Wrap CatanDiceSystem**
```csharp
public class EnhancedCatanDiceSystem
{
    private CatanDiceSystem catanDice;
    
    public EnhancedCatanDiceSystem()
    {
        catanDice = new CatanDiceSystem();
    }
    
    public CatanDiceRolledEvent RollWithHistory()
    {
        CatanDiceRolledEvent roll = catanDice.Roll();
        // Track in history
        return roll;
    }
}
```

### Custom Event Handling

**Create Extended Event:**
```csharp
public class ExtendedCatanDiceEvent : CatanDiceRolledEvent
{
    public bool IsDouble { get; }
    
    public ExtendedCatanDiceEvent(DiceRollResult baseResult) 
        : base(baseResult)
    {
        IsDouble = FirstDie == SecondDie;
    }
}
```

---

## Integration with Existing Code

### Replacing DicesController

**Old Code (MareGameplay.DicesController):**
```csharp
// Old way
dicesController.Roll();
```

**New Code:**
```csharp
// New way
CatanDiceSystem catanDice = ServiceLocator.Get<CatanDiceSystem>();
CatanDiceRolledEvent rollEvent = catanDice.Roll();

// Update UI
firstDiceText.text = rollEvent.FirstDie.ToString();
secondDiceText.text = rollEvent.SecondDie.ToString();
```

### UI Integration

**Update UI on Roll:**
```csharp
public class DiceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI firstDiceText;
    [SerializeField] private TextMeshProUGUI secondDiceText;
    [SerializeField] private TextMeshProUGUI totalText;
    
    private void OnEnable()
    {
        EventBus.Subscribe<CatanDiceRolledEvent>(OnDiceRolled);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe<CatanDiceRolledEvent>(OnDiceRolled);
    }
    
    private void OnDiceRolled(CatanDiceRolledEvent evt)
    {
        firstDiceText.text = evt.FirstDie.ToString();
        secondDiceText.text = evt.SecondDie.ToString();
        totalText.text = evt.Total.ToString();
        
        if (evt.IsRobber)
        {
            // Show robber indicator
        }
    }
}
```

---

## Best Practices

1. **Use ServiceLocator**: Register and retrieve via ServiceLocator for easy access
2. **Subscribe to Events**: Use events for decoupled dice handling
3. **Check IsRobber**: Always check `IsRobber` property for robber activation
4. **Unregister**: Call `Unregister()` when disposing of the system
5. **Seed for Testing**: Use seed parameter for deterministic testing

---

## Testing

Use the `CatanDiceSystemTest` script to validate functionality:
1. Add `CatanDiceSystemTest` component to a GameObject
2. Run the scene
3. Check console for test results

All tests should pass:
- ✅ Roll
- ✅ Robber Activation
- ✅ Event Publishing
- ✅ Service Locator

---

## Catan Rules Implementation

### Dice Rules
- **Always 2 dice**: Cannot be changed
- **Always 6 sides**: Cannot be changed
- **Range**: 2-12 (1+1 to 6+6)
- **Robber**: Activated when total = 7

### Robber Activation
When a 7 is rolled:
- `IsRobber` property is true
- Robber mechanics should be triggered
- Resource distribution is blocked
- Players with 7+ resources must discard half

---

## Version History

- **1.0.0** (2024-12-19): Initial release
  - Catan dice system implementation
  - Robber detection
  - Event publishing
  - ServiceLocator integration

---

## Support

For issues or questions, refer to the main project documentation or create an issue in the repository.

