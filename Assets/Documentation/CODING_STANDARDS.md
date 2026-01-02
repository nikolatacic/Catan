# Catan Unity Project - Coding Standards

## Overview
This document defines the coding standards and best practices for the Catan Unity project. All code should follow these guidelines to ensure consistency, maintainability, and reusability.

**Last Updated**: [Will be updated as we progress]

---

## General Principles

1. **Clarity Over Cleverness**: Code should be easy to read and understand
2. **Consistency**: Follow established patterns throughout the project
3. **Reusability**: Design with extraction to packages in mind
4. **Testability**: Write code that can be easily tested
5. **Documentation**: Document public APIs and complex logic

---

## Naming Conventions

### Classes
- **Format**: PascalCase
- **Examples**: `DiceSystem`, `CatanDiceSystem`, `GameManager`
- **Rules**: 
  - One class per file
  - File name must match class name exactly

### Interfaces
- **Format**: I + PascalCase
- **Examples**: `IDiceSystem`, `IEvent`, `ICardSystem`
- **Rules**: 
  - Start with "I"
  - Descriptive name

### Methods
- **Format**: PascalCase
- **Examples**: `RollDice()`, `PlaceBuilding()`, `GetResourceCount()`
- **Rules**: 
  - Use verb-noun pattern
  - Be descriptive

### Events
- **Format**: PascalCase + "Event"
- **Examples**: `DiceRolledEvent`, `BuildingPlacedEvent`, `ResourceChangedEvent`
- **Rules**: 
  - Must end with "Event"
  - Use past tense for action events (Rolled, Placed)
  - Use "Changed" for state events (ResourceChanged, StateChanged)

### Private Fields
- **Format**: camelCase (no underscore prefix)
- **Examples**: `instance`, `currentPlayer`, `diceSystem`
- **Rules**: 
  - Use camelCase without underscore
  - Use `this.` prefix if needed to distinguish from parameters

### Properties
- **Format**: PascalCase
- **Examples**: `CurrentPlayer`, `IsGameActive`, `DiceCount`
- **Rules**: 
  - Public properties use PascalCase
  - Private setters when appropriate

### Constants
- **Format**: UPPER_SNAKE_CASE
- **Examples**: `MAX_PLAYERS`, `DEFAULT_RESOURCES`, `MIN_DICE_VALUE`
- **Rules**: 
  - All caps
  - Underscores between words
  - Use `const` or `static readonly`

### Enums
- **Format**: PascalCase
- **Examples**: `GamePhase`, `ResourceType`, `BuildingType`
- **Rules**: 
  - Enum name: PascalCase
  - Enum values: PascalCase

### Local Variables
- **Format**: camelCase
- **Examples**: `playerCount`, `diceResult`, `isValid`
- **Rules**: 
  - camelCase
  - Descriptive names

### Parameters
- **Format**: camelCase
- **Examples**: `playerId`, `resourceType`, `buildingPosition`
- **Rules**: 
  - camelCase
  - Descriptive names

---

## Code Organization

### File Structure
```
[Namespace declarations]
[Using statements]
[Class definition]
  [Constants]
  [Fields]
  [Properties]
  [Constructors]
  [Public Methods]
  [Private Methods]
  [Event Handlers]
```

### Class Organization
1. Constants (if any)
2. Private fields
3. Public properties
4. Constructors/Initialization
5. Public methods
6. Private methods
7. Event handlers
8. Unity lifecycle methods (if MonoBehaviour)

### Namespace Usage
- **Infrastructure**: No namespace (or `Infrastructure`)
- **Core Systems**: `CoreSystems` or `CoreSystems.Dice`
- **Game Systems**: `GameSystems` or `GameSystems.CatanDice`
- **Game-Specific**: `Catan` or `Catan.Managers`
- **Presentation**: `Presentation` or `Presentation.UI`

---

## Code Style

### Indentation
- **Size**: 4 spaces (Unity default)
- **Tabs**: Never use tabs, only spaces

### Braces
- **Opening brace**: Same line
- **Closing brace**: Own line
- **Always use braces**: Even for single-line statements

```csharp
// ✅ Good
if (condition)
{
    DoSomething();
}

// ❌ Bad
if (condition)
    DoSomething();

// ❌ Bad
if (condition) { DoSomething(); }
```

### Line Length
- **Maximum**: 120 characters (soft limit)
- **Preferred**: 80-100 characters
- **Break long lines**: At logical points

### Spacing
- **Around operators**: Yes
- **After commas**: Yes
- **Around braces**: Yes
- **Method parameters**: Space after comma, not before

```csharp
// ✅ Good
int result = a + b;
if (condition && otherCondition)
{
    Method(param1, param2);
}

// ❌ Bad
int result=a+b;
if(condition&&otherCondition){
    Method(param1,param2);
}
```

### Variable Declaration
- **Avoid `var` keyword**: Use explicit types for clarity
- **Exception**: Only use `var` when type is extremely obvious from context (rare)

```csharp
// ✅ Good - explicit types
int playerCount = GetPlayerCount();
List<Player> players = GetPlayers();
string playerName = player?.Name ?? "Unknown";

// ❌ Bad - using var
var playerCount = GetPlayerCount();
var players = GetPlayers();
var playerName = player?.Name ?? "Unknown";
```

---

## Comments & Documentation

### XML Documentation
Use XML comments for all public APIs:

```csharp
/// <summary>
/// Rolls the specified number of dice and returns the result.
/// </summary>
/// <param name="diceCount">Number of dice to roll</param>
/// <returns>DiceRollResult containing individual die values and total</returns>
public DiceRollResult Roll(int diceCount)
{
    // Implementation
}
```

### Inline Comments
- **When to use**: Explain WHY, not WHAT
- **Complex logic**: Always comment
- **Non-obvious code**: Always comment
- **TODOs**: Use TODO comments for future work

```csharp
// ✅ Good - explains why
// Using Fisher-Yates shuffle for true randomness
ShuffleDeck();

// ❌ Bad - explains what (code already does this)
// Shuffle the deck
ShuffleDeck();
```

### Commented-Out Code
- **Rule**: Remove before committing
- **Exception**: Temporary debugging (remove after fixing)

---

## Null Safety

### Null Checks
Always check for null before using objects:

```csharp
// ✅ Good
if (player != null)
{
    player.AddResources(resources);
}

// ✅ Good - null-conditional
player?.AddResources(resources);

// ❌ Bad
player.AddResources(resources); // Could throw NullReferenceException
```

### Null-Conditional Operator
Use `?.` when appropriate:

```csharp
// ✅ Good
string name = player?.Name ?? "Unknown";
int count = players?.Count ?? 0;
```

### Null-Coalescing Operator
Use `??` for defaults:

```csharp
// ✅ Good
ResourceType resource = GetResource() ?? ResourceType.Wood;
int count = GetCount() ?? 0;
```

### Parameter Validation
Validate parameters in public methods:

```csharp
// ✅ Good
public void PlaceBuilding(BuildingType type, Vector3 position)
{
    if (type == null)
    {
        throw new ArgumentNullException(nameof(type));
    }
    
    if (position == Vector3.zero)
    {
        throw new ArgumentException("Position cannot be zero", nameof(position));
    }
    
    // Implementation
}
```

---

## LINQ Usage

### When to Use LINQ
- **Filtering**: `Where()`, `FirstOrDefault()`, `Any()`
- **Mapping**: `Select()`, `SelectMany()`
- **Aggregation**: `Sum()`, `Count()`, `Average()`
- **Simplification**: When LINQ makes code clearer

### When NOT to Use LINQ
- **Performance-critical loops**: Use traditional loops
- **Simple operations**: When loop is clearer
- **Update 2024-12-19 16:00:00**: When modifying collections

### Examples

```csharp
// ✅ Good - LINQ simplifies
List<Player> playersWithResources = players.Where(p => p.HasResources()).ToList();
int totalResources = players.Sum(p => p.ResourceCount);

// ✅ Good - Traditional loop is clearer
foreach (Player player in players)
{
    player.ProcessTurn();
}

// ❌ Bad - LINQ used unnecessarily
players.Select(p => p.ProcessTurn()).ToList(); // ProcessTurn() has side effects
```

---

## Error Handling

### Exception Types
- **ArgumentNullException**: Null parameters
- **ArgumentException**: Invalid parameters
- **InvalidOperationException**: Invalid state
- **Custom exceptions**: For domain-specific errors

### Error Messages
- **Be descriptive**: Explain what went wrong
- **Include context**: Relevant values, state
- **Be actionable**: Suggest how to fix

```csharp
// ✅ Good
if (diceCount < 1)
{
    throw new ArgumentException($"Dice count must be at least 1, but was {diceCount}", nameof(diceCount));
}

// ❌ Bad
if (diceCount < 1)
{
    throw new Exception("Invalid");
}
```

### Logging
- **Debug.Log**: Informational messages
- **Debug.LogWarning**: Recoverable issues
- **Debug.LogError**: Critical errors
- **Use Logger**: When available, use Logger system

```csharp
// ✅ Good
Logger?.LogWarning($"Player {playerId} attempted invalid action");
Debug.LogError($"Critical error in {systemName}: {errorMessage}");

// ❌ Bad
Debug.Log("Error"); // Too vague
```

---

## Unity-Specific Guidelines

### MonoBehaviour Lifecycle
- **Awake()**: Initialize singleton, register services
- **Start()**: Initialize non-singleton components
- **OnEnable()**: Subscribe to events
- **OnDisable()**: Unsubscribe from events
- **OnDestroy()**: Cleanup resources

### Serialized Fields
- **Use [SerializeField]**: For private fields shown in Inspector
- **Public fields**: Only for simple, non-critical data
- **Properties**: Prefer properties over public fields

```csharp
// ✅ Good
[SerializeField] private int diceCount;
[SerializeField] private DiceConfig config;

// ⚠️ Acceptable (simple cases)
public int MaxPlayers = 4;

// ✅ Better
public int MaxPlayers { get; private set; } = 4;
```

### Coroutines vs Async
- **Coroutines**: For time-based operations, Unity-specific
- **Async/Await**: For I/O operations, non-Unity code
- **Don't mix**: Avoid mixing unnecessarily

### Object References
- **Avoid FindObjectOfType**: Use ServiceLocator or dependency injection
- **Cache references**: Store in fields, not retrieved every frame
- **Null checks**: Always check before use

---

## Event System Guidelines

### Event Subscription
- **Subscribe in OnEnable()**: Not in constructor or Start()
- **Unsubscribe in OnDisable()**: Prevent memory leaks
- **Check for null**: Before invoking handlers

```csharp
// ✅ Good
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
    // Handle event
}
```

### Event Data
- **Keep minimal**: Only necessary data
- **Use IDs**: Prefer IDs over full objects
- **Immutable**: Make events immutable (readonly properties)

```csharp
// ✅ Good
public class DiceRolledEvent : IEvent
{
    public int Total { get; }
    public int[] IndividualResults { get; }
    public float Timestamp { get; }
    
    public DiceRolledEvent(int total, int[] results)
    {
        Total = total;
        IndividualResults = results;
        Timestamp = Time.time;
    }
}

// ❌ Bad
public class DiceRolledEvent : IEvent
{
    public DiceSystem FullSystem { get; } // Too much data
    public GameObject DiceObject { get; } // Unity dependency
}
```

---

## Testing Guidelines

### Unit Tests
- **Test models**: Pure C# classes, no Unity
- **Test logic**: Validation, calculations
- **Test edge cases**: Boundary conditions
- **Arrange-Act-Assert**: Follow AAA pattern

### Integration Tests
- **Test interactions**: Between systems
- **Test events**: Event flow
- **Test state**: State transitions

### Test Naming
- **Format**: `MethodName_Scenario_ExpectedResult`
- **Example**: `RollDice_WithTwoDice_ReturnsSumBetween2And12`

---

## Performance Guidelines

### Avoid Allocations in Update()
- **Cache references**: Don't get components every frame
- **Use object pooling**: For frequently created/destroyed objects
- **Minimize LINQ**: In performance-critical code
- **StringBuilder**: For string concatenation in loops

### Event Optimization
- **Unsubscribe**: When not needed
- **Minimize subscriptions**: Only subscribe to needed events
- **Cache event handlers**: Avoid lambda allocations

### Profiling
- **Profile before optimizing**: Don't optimize prematurely
- **Use Unity Profiler**: Identify bottlenecks
- **Measure**: Verify improvements

---

## Code Review Checklist

Before submitting code, check:
- [ ] Follows naming conventions
- [ ] Properly organized (fields, properties, methods)
- [ ] XML documentation on public APIs
- [ ] Null checks where needed
- [ ] Error handling appropriate
- [ ] Events subscribed/unsubscribed correctly
- [ ] No commented-out code
- [ ] No magic numbers (use constants)
- [ ] Code is testable
- [ ] Follows architecture patterns

---

## Examples

### Good Example

```csharp
using System;
using UnityEngine;

namespace CoreSystems.Dice
{
    /// <summary>
    /// Generic dice rolling system that can be used in any game.
    /// </summary>
    public class DiceSystem : IDiceSystem
    {
        private const int DEFAULT_SIDES = 6;
        
        private readonly System.Random _random;
        private DiceRollResult _lastResult;
        
        public DiceRollResult LastResult => _lastResult;
        
        public DiceSystem()
        {
            _random = new System.Random();
        }
        
        /// <summary>
        /// Rolls the specified number of dice with default 6 sides.
        /// </summary>
        public DiceRollResult Roll(int diceCount)
        {
            if (diceCount < 1)
            {
                throw new ArgumentException($"Dice count must be at least 1, but was {diceCount}", nameof(diceCount));
            }
            
            return Roll(diceCount, DEFAULT_SIDES);
        }
        
        /// <summary>
        /// Rolls the specified number of dice with the given number of sides.
        /// </summary>
        public DiceRollResult Roll(int diceCount, int sides)
        {
            if (diceCount < 1)
            {
                throw new ArgumentException($"Dice count must be at least 1, but was {diceCount}", nameof(diceCount));
            }
            
            if (sides < 2)
            {
                throw new ArgumentException($"Dice sides must be at least 2, but was {sides}", nameof(sides));
            }
            
            int[] results = new int[diceCount];
            int total = 0;
            
            for (int i = 0; i < diceCount; i++)
            {
                results[i] = _random.Next(1, sides + 1);
                total += results[i];
            }
            
            _lastResult = new DiceRollResult(results, total);
            
            EventBus.Publish(new DiceRolledEvent(_lastResult));
            
            return _lastResult;
        }
    }
}
```

---

**Last Updated**: [Will be updated as we progress]

