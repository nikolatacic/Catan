# Catan Unity Project - Architecture Documentation

## Overview
This document describes the complete architecture of the Catan Unity project. The architecture is designed with reusability in mind, allowing systems to be extracted into packages for use in future projects.

**Last Updated**: [Will be updated as we progress]  
**Current Phase**: Phase 0 - Project Setup

---

## Architecture Layers

### Layer 1: Infrastructure
**Purpose**: Foundation with zero dependencies  
**Reusability**: Maximum - can be extracted immediately  
**Dependencies**: None

**Components**:
- EventBus: Decoupled event communication system
- ServiceLocator: Central service registry
- Logger: Centralized logging (optional)

**Design Principles**:
- Pure C# classes
- No Unity dependencies
- Thread-safe if needed
- No game logic

---

### Layer 2: Core Systems
**Purpose**: Generic, reusable game systems  
**Reusability**: High - game-agnostic  
**Dependencies**: Infrastructure only

**Components**:
- DiceSystem: Generic dice rolling
- CardSystem: Deck and hand management
- TradingSystem: Resource/item exchange
- ResourceSystem: Inventory management
- TurnSystem: Turn order and phases
- TimerSystem: Countdown timers

**Design Principles**:
- Depend only on Infrastructure
- Use interfaces for extensibility
- Configuration via ScriptableObjects
- Event-driven communication
- No game-specific logic

---

### Layer 3: Game Systems
**Purpose**: Catan-specific implementations  
**Reusability**: Medium - Catan-specific but structured  
**Dependencies**: Core Systems + Infrastructure

**Components**:
- CatanDiceSystem: Wraps DiceSystem, adds Catan rules
- CatanCardSystem: Development cards
- CatanTradingSystem: Port and player trading
- CatanResourceSystem: Catan resource distribution
- BuildingSystem: Building placement and validation
- RobberSystem: Robber mechanics
- VictorySystem: Victory point tracking

**Design Principles**:
- Compose core systems (don't inherit)
- Implement game-specific rules
- Publish game-specific events
- Register with ServiceLocator

---

### Layer 4: Game-Specific
**Purpose**: Catan game flow and coordination  
**Reusability**: Low - Catan-specific  
**Dependencies**: Game Systems + Core Systems + Infrastructure

**Components**:
- GameManager: Game state machine and flow
- PlayerManager: Player lifecycle
- MapManager: Map generation coordination
- GameState: Current game state

**Design Principles**:
- Orchestrates systems
- Manages game flow
- Coordinates between systems
- Uses ServiceLocator for system access

---

### Layer 5: Presentation
**Purpose**: UI and visual representation  
**Reusability**: Low - game-specific visuals  
**Dependencies**: All layers below

**Components**:
- UIManager: UI coordination
- BuildingPlaceholder: Visual building placement
- DiceUI: Dice visualization
- ResourceUI: Resource display
- CameraController: Camera management

**Design Principles**:
- Subscribe to events for updates
- Publish user actions as events
- No game logic
- Pure presentation

---

## Dependency Rules

### Rule 1: One-Way Dependencies
Lower layers NEVER depend on higher layers.
```
Infrastructure → Core Systems → Game Systems → Game-Specific → Presentation
```

### Rule 2: Horizontal Communication via Events
Systems in the same layer communicate via EventBus, not direct references.

### Rule 3: Service Locator for Read Access
Systems can query ServiceLocator for other systems, but should use events for actions.

### Rule 4: Composition Over Inheritance
Game systems wrap core systems, they don't inherit from them.

---

## Communication Patterns

### Pattern 1: Event-Driven (Preferred)
```
System A publishes event → EventBus → System B subscribes
```
- Decoupled
- Multiple subscribers
- Easy to extend

### Pattern 2: Service Locator (For Queries)
```
System A needs data → ServiceLocator.Get<SystemB>() → Query data
```
- For read-only access
- Immediate data needs
- Not for triggering actions

### Pattern 3: Direct Composition (Within System)
```
CatanDiceSystem contains DiceSystem instance
```
- Internal to a system
- Not for cross-system communication

---

## Folder Structure

```
Assets/
├── Infrastructure/              # Package-ready
│   ├── EventBus/
│   │   ├── EventBus.cs
│   │   ├── IEvent.cs
│   │   └── BaseEvent.cs (optional)
│   ├── ServiceLocator/
│   │   └── ServiceLocator.cs
│   └── Logger/
│       ├── ILogger.cs
│       └── UnityLogger.cs
│
├── CoreSystems/                 # Package-ready
│   ├── Dice/
│   │   ├── IDiceSystem.cs
│   │   ├── DiceSystem.cs
│   │   ├── DiceRollResult.cs
│   │   ├── DiceConfig.cs (ScriptableObject)
│   │   └── Events/
│   │       └── DiceRolledEvent.cs
│   ├── Cards/
│   ├── Trading/
│   ├── Resources/
│   ├── TurnManagement/
│   └── Timer/
│
├── GameSystems/                 # Catan-specific, structured
│   ├── CatanDice/
│   │   ├── CatanDiceSystem.cs
│   │   └── Events/
│   │       └── CatanDiceRolledEvent.cs
│   ├── CatanCards/
│   ├── CatanTrading/
│   ├── Building/
│   ├── Robber/
│   └── Victory/
│
├── GameSpecific/                # Catan implementation
│   ├── Managers/
│   │   ├── GameManager.cs
│   │   ├── PlayerManager.cs
│   │   └── MapManager.cs
│   ├── Data/
│   │   ├── CatanGameState.cs
│   │   └── CatanPlayerData.cs
│   └── Rules/
│       └── CatanRuleValidator.cs
│
├── Presentation/                # Visual layer
│   ├── UI/
│   │   ├── UIManager.cs
│   │   ├── DiceUI.cs
│   │   ├── ResourceUI.cs
│   │   └── BuildingUI.cs
│   ├── Visuals/
│   │   ├── BuildingPlaceholder.cs
│   │   └── RoadPlaceholder.cs
│   └── Camera/
│       └── CameraController.cs
│
└── ScriptableObjects/           # Configuration
    ├── DiceConfig.asset
    ├── CardConfig.asset
    └── GameConfig.asset
```

---

## Event System Architecture

### Event Categories

1. **Infrastructure Events** (Infrastructure layer)
   - Base events, event bus events

2. **Core System Events** (Core Systems layer)
   - `DiceRolledEvent` - Generic, reusable
   - `CardDrawnEvent` - Generic, reusable
   - `TradeCompletedEvent` - Generic, reusable
   - `ResourceChangedEvent` - Generic, reusable

3. **Game System Events** (Game Systems layer)
   - `CatanDiceRolledEvent` - Catan-specific
   - `BuildingPlacedEvent` - Catan-specific
   - `RobberPlacedEvent` - Catan-specific
   - `ResourceDistributedEvent` - Catan-specific

4. **Game State Events** (Game-Specific layer)
   - `GameStateChangedEvent` - Game flow
   - `TurnStartedEvent` - Turn management
   - `PlayerChangedEvent` - Player management

5. **UI Events** (Presentation layer)
   - `ButtonClickedEvent` - User input
   - `PlaceholderSelectedEvent` - User interaction

### Event Flow Example

**Dice Roll Flow**:
1. User clicks "Roll Dice" button
2. UI publishes `DiceRollRequestedEvent`
3. `CatanDiceSystem` subscribes, calls `DiceSystem.Roll()`
4. `DiceSystem` publishes `DiceRolledEvent` with result
5. `CatanDiceSystem` processes (checks for 7, triggers robber)
6. `CatanDiceSystem` publishes `CatanDiceRolledEvent`
7. UI subscribes and updates dice display
8. Resource system subscribes and distributes resources

---

## System Design Patterns

### Singleton Pattern
- **Use for**: Managers that need Unity lifecycle
- **Implementation**: MonoBehaviour singleton with Instance property
- **Initialization**: In Awake(), not Start()
- **Access**: ManagerName.Instance.Method()

### Service Locator Pattern
- **Use for**: Centralized access to systems
- **Implementation**: Static ServiceLocator class
- **Registration**: Systems register themselves
- **Access**: ServiceLocator.Get<SystemType>()

### Observer Pattern
- **Use for**: Event-driven communication
- **Implementation**: EventBus
- **Subscription**: In OnEnable()
- **Unsubscription**: In OnDisable()

### Composition Pattern
- **Use for**: Wrapping core systems
- **Implementation**: Game systems contain core system instances
- **Example**: CatanDiceSystem contains DiceSystem

---

## Coding Standards

### Naming Conventions
- **Classes**: PascalCase (PlayerManager, DiceSystem)
- **Methods**: PascalCase (RollDice, PlaceBuilding)
- **Events**: PascalCase + "Event" (DiceRolledEvent)
- **Private fields**: camelCase with underscore (_instance, _currentPlayer)
- **Properties**: PascalCase (CurrentPlayer, IsGameActive)
- **Constants**: UPPER_SNAKE_CASE (MAX_PLAYERS, DEFAULT_RESOURCES)
- **Enums**: PascalCase (GamePhase, ResourceType)
- **Interfaces**: I + PascalCase (IDiceSystem, IEvent)

### Code Organization
- One class per file
- File name matches class name
- Use namespaces for logical grouping
- Keep classes focused (single responsibility)
- Maximum class size: ~300 lines
- Public methods first, private methods after

### Comments & Documentation
- XML comments for public methods/classes
- Explain WHY, not WHAT
- Comment complex algorithms
- Remove commented-out code
- Use TODO comments for future work

### Null Safety
- Always check for null before using objects
- Use null-conditional operator (?.) when appropriate
- Use null-coalescing operator (??) for defaults
- Validate parameters in public methods

---

## Testing Strategy

### Unit Testing
- Test models independently (no Unity dependencies)
- Test validation logic
- Test edge cases
- Test error conditions

### Integration Testing
- Test system interactions
- Test event flow
- Test complete game scenarios
- Test state transitions

### Manual Testing Checklist
For each phase:
1. Happy path (normal gameplay)
2. Edge cases (boundary conditions)
3. Error cases (invalid inputs)
4. State transitions
5. Event publishing/subscribing
6. Memory leaks (unsubscribe properly)

---

## Package Extraction Strategy

### Phase 1: Identify Candidates
After implementing systems, identify what's truly reusable:
- Systems used in multiple places
- Systems with minimal game-specific logic
- Systems that work independently

### Phase 2: Extract Incrementally
- Start with most generic (Infrastructure, Dice, Timer)
- Extract one system at a time
- Test in Catan after each extraction

### Phase 3: Create Package Structure
```
PackageName/
├── Runtime/
│   └── [System files]
├── Editor/
│   └── [Editor tools]
├── Tests/
│   └── [Test files]
└── package.json
```

### Phase 4: Version Management
- Use semantic versioning
- Document breaking changes
- Maintain changelog

---

## Migration from Existing Code

### Existing Systems to Refactor
- `DicesController` → Will become CatanDiceSystem wrapper
- `CardController` → Will become CatanCardSystem wrapper
- `TimerController` → Will become TimerSystem wrapper
- `PlaceholderElement` → Will integrate with BuildingSystem
- `GameplayManager` → Will become GameManager
- `MapGenerator` → Will stay in GameSpecific, may extract later

### Refactoring Strategy
1. Build new architecture alongside old code
2. Replace old systems one at a time
3. Test after each replacement
4. Remove old code once new system is proven

---

## Future Considerations

### Potential Extensions
- Save/Load system
- Network multiplayer
- AI players
- Replay system
- Analytics

### Performance Optimization
- Object pooling for frequently created objects
- Event subscription optimization
- Caching frequently accessed data
- Profiling and optimization

---

## References

- [PROJECT_PHASES.md](./PROJECT_PHASES.md) - Implementation phases
- [PROGRESS.md](./PROGRESS.md) - Current progress tracking
- [CODING_STANDARDS.md](./CODING_STANDARDS.md) - Detailed coding standards

