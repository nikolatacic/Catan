# Dice System Package

## Overview
The Dice System provides a generic, reusable dice rolling system that can work in any game requiring dice. It's completely game-agnostic and can be configured for various dice types and scenarios.

**Package Name**: `com.yourcompany.coresystems` (Dice module)  
**Version**: 1.0.0  
**Dependencies**: Infrastructure package (`com.yourcompany.infrastructure`)

---

## What It Does

The Dice System allows you to:
- Roll any number of dice with any number of sides
- Configure dice behavior via ScriptableObjects
- Track roll results and history
- Receive events when dice are rolled
- Use in any game requiring dice mechanics

---

## How to Use

### Basic Usage

**Simple Roll:**
```csharp
// Create dice system
IDiceSystem diceSystem = new DiceSystem();

// Roll 2 dice with default 6 sides
DiceRollResult result = diceSystem.Roll(2);
// result.Total = sum of both dice
// result.IndividualResults = [die1, die2]
```

**Custom Sides:**
```csharp
// Roll 1 die with 20 sides (D&D style)
DiceRollResult result = diceSystem.Roll(1, 20);
```

**Using Configuration:**
```csharp
// Get your DiceConfig (created in Unity Editor)
DiceConfig config = Resources.Load<DiceConfig>("DiceConfigs/StandardDice");

// Roll using configuration
DiceRollResult result = diceSystem.Roll(config);
```

### Listening to Events

**Subscribe to Dice Rolls:**
```csharp
private void OnEnable()
{
    EventBus.Subscribe<DiceRolledEvent>(OnDiceRolled);
}

private void OnDisable()
{
    EventBus.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
}

private void OnDiceRolled(DiceRolledEvent evt)
{
    Debug.Log($"Dice rolled: {evt.Result.Total}");
    // Handle the roll result
}
```

### Using ServiceLocator

**Register Dice System:**
```csharp
// In initialization
IDiceSystem diceSystem = new DiceSystem();
ServiceLocator.Register<IDiceSystem>(diceSystem);
```

**Get Dice System:**
```csharp
// Anywhere in your code
IDiceSystem diceSystem = ServiceLocator.Get<IDiceSystem>();
DiceRollResult result = diceSystem.Roll(2, 6);
```

---

## ScriptableObject Configuration

### Creating a DiceConfig

1. **In Unity Editor**: Right-click in Project window
2. **Select**: `Create > Core Systems > Dice Config`
3. **Name it**: e.g., "StandardDice", "D20Dice", "CatanDice"
4. **Configure**:
   - **Dice Count**: Number of dice to roll
   - **Sides**: Number of sides per die (used if custom values are not set)
   - **Custom Face Values**: Optional list of specific values (e.g., [1, 1, 4] for a 3-sided die with those exact values)
   - **Modifier**: Value to add to total (applied in game logic)
   - **Min Value**: Minimum value per die (ignored if custom values are set)
   - **Max Value**: Maximum value per die (ignored if custom values are set)

### Example Configurations

**Standard 2d6 (Catan):**
- Dice Count: 2
- Sides: 6
- Custom Face Values: (empty - uses standard 1-6)
- Modifier: 0
- Min Value: 1
- Max Value: 6

**D&D d20:**
- Dice Count: 1
- Sides: 20
- Custom Face Values: (empty - uses standard 1-20)
- Modifier: 0
- Min Value: 1
- Max Value: 20

**Custom Face Values (3-sided die with values 1, 1, 4):**
- Dice Count: 1
- Sides: 3 (informational, not used)
- Custom Face Values: [1, 1, 4]
- Modifier: 0
- Each roll will randomly pick one value from the list

**Weighted Dice (more 1s than other values):**
- Dice Count: 1
- Sides: 6
- Custom Face Values: [1, 1, 1, 2, 3, 4, 5, 6]
- Modifier: 0
- Higher chance of rolling 1 (3 out of 8 possibilities)

**Custom Range (without custom values):**
- Dice Count: 3
- Sides: 6
- Custom Face Values: (empty)
- Modifier: 0
- Min Value: 2 (minimum roll is 2)
- Max Value: 5 (maximum roll is 5)

### Using Config in Code

**Load from Resources:**
```csharp
DiceConfig config = Resources.Load<DiceConfig>("DiceConfigs/StandardDice");
DiceRollResult result = diceSystem.Roll(config);
```

**Reference in Inspector:**
```csharp
public class MyGameManager : MonoBehaviour
{
    [SerializeField] private DiceConfig diceConfig;
    
    private void RollDice()
    {
        IDiceSystem diceSystem = ServiceLocator.Get<IDiceSystem>();
        DiceRollResult result = diceSystem.Roll(diceConfig);
    }
}
```

**Create Programmatically:**
```csharp
// Standard dice
DiceConfig config = ScriptableObject.CreateInstance<DiceConfig>();
config.DiceCount = 2;
config.Sides = 6;
config.Modifier = 0;

DiceRollResult result = diceSystem.Roll(config);

// Custom face values
DiceConfig customConfig = ScriptableObject.CreateInstance<DiceConfig>();
customConfig.DiceCount = 1;
customConfig.CustomFaceValues = new List<int> { 1, 1, 4 };
// Sides is ignored when custom values are set

DiceRollResult customResult = diceSystem.Roll(customConfig);
```

---

## How to Modify/Upgrade/Extend

### Creating Custom Dice System

**Option 1: Implement IDiceSystem**
```csharp
public class CustomDiceSystem : IDiceSystem
{
    // Implement all interface methods
    public DiceRollResult Roll(int diceCount) { /* Custom logic */ }
    public DiceRollResult Roll(int diceCount, int sides) { /* Custom logic */ }
    public DiceRollResult Roll(DiceConfig config) { /* Custom logic */ }
    public DiceRollResult GetLastResult() { /* Custom logic */ }
    public void Reset() { /* Custom logic */ }
}
```

**Option 2: Extend DiceSystem**
```csharp
public class CustomDiceSystem : DiceSystem
{
    public CustomDiceSystem(int? seed = null) : base(seed) { }
    
    // Override or add methods
    public DiceRollResult RollWithAdvantage()
    {
        DiceRollResult roll1 = Roll(1, 20);
        DiceRollResult roll2 = Roll(1, 20);
        return roll1.Total > roll2.Total ? roll1 : roll2;
    }
}
```

**Option 3: Wrap DiceSystem (Recommended)**
```csharp
public class GameSpecificDiceSystem
{
    private IDiceSystem diceSystem;
    
    public GameSpecificDiceSystem()
    {
        diceSystem = new DiceSystem();
    }
    
    public DiceRollResult RollForGame()
    {
        // Add game-specific logic
        DiceRollResult result = diceSystem.Roll(2, 6);
        
        // Process result
        if (result.Total == 7)
        {
            // Handle special case
        }
        
        return result;
    }
}
```

### Extending DiceRollResult

**Create Custom Result:**
```csharp
public class ExtendedDiceResult : DiceRollResult
{
    public bool IsCritical { get; }
    public bool IsFumble { get; }
    
    // Note: DiceRollResult doesn't have a public constructor
    // You'd need to create a wrapper or modify the base class
}
```

**Better Approach - Wrapper:**
```csharp
public class GameDiceResult
{
    public DiceRollResult BaseResult { get; }
    public bool IsCritical { get; }
    public bool IsFumble { get; }
    
    public GameDiceResult(DiceRollResult baseResult)
    {
        BaseResult = baseResult;
        IsCritical = baseResult.Total >= 18;
        IsFumble = baseResult.Total <= 3;
    }
}
```

### Creating Custom Events

**Extend DiceRolledEvent:**
```csharp
public class GameDiceRolledEvent : DiceRolledEvent
{
    public bool IsSpecialRoll { get; }
    
    public GameDiceRolledEvent(DiceRollResult result, bool isSpecial) 
        : base(result)
    {
        IsSpecialRoll = isSpecial;
    }
}
```

---

## Advanced Usage

### Seeded Random (For Testing/Replay)

```csharp
// Create dice system with seed for deterministic rolls
IDiceSystem diceSystem = new DiceSystem(seed: 12345);
DiceRollResult result = diceSystem.Roll(2, 6);
// Same seed = same results
```

### Tracking Roll History

```csharp
public class DiceHistory
{
    private List<DiceRollResult> history = new List<DiceRollResult>();
    
    public DiceHistory()
    {
        EventBus.Subscribe<DiceRolledEvent>(OnDiceRolled);
    }
    
    private void OnDiceRolled(DiceRolledEvent evt)
    {
        history.Add(evt.Result);
    }
    
    public List<DiceRollResult> GetHistory()
    {
        return new List<DiceRollResult>(history);
    }
}
```

### Applying Modifiers

```csharp
public int GetModifiedTotal(DiceRollResult result, int modifier)
{
    return result.Total + modifier;
}

// Or in your game logic
DiceRollResult result = diceSystem.Roll(2, 6);
int finalTotal = result.Total + characterModifier;
```

---

## Best Practices

1. **Use Interface**: Always use `IDiceSystem` interface, not `DiceSystem` class directly
2. **Register with ServiceLocator**: Register dice system for easy access
3. **Subscribe to Events**: Use events for decoupled dice handling
4. **Use Configs**: Create ScriptableObject configs for different dice types
5. **Reset When Needed**: Call `Reset()` when starting new game/round
6. **Validate Results**: Check result ranges in your game logic

---

## Testing

Use the `DiceSystemTest` script to validate functionality:
1. Add `DiceSystemTest` component to a GameObject
2. Run the scene
3. Check console for test results

All tests should pass:
- ✅ Basic Roll
- ✅ Custom Sides
- ✅ Config Roll
- ✅ Event Publishing
- ✅ Last Result
- ✅ Reset

---

## Package Extraction

When extracting to a package:
1. Copy `CoreSystems/Dice/` folder to package structure
2. Create `package.json` with Infrastructure dependency
3. Update namespaces if needed
4. Test package in isolation
5. Ensure Infrastructure package is available

---

## Version History

- **1.0.0** (2024-12-19): Initial release
  - Generic dice rolling system
  - ScriptableObject configuration
  - Event publishing
  - Result tracking

---

## Support

For issues or questions, refer to the main project documentation or create an issue in the repository.

