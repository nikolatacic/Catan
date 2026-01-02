# Catan Unity Project - Implementation Phases

## Architecture Overview
This project follows a layered architecture designed for reusability:
- **Infrastructure Layer**: Foundation (EventBus, ServiceLocator)
- **Core Systems Layer**: Reusable game systems (Dice, Cards, Trading, etc.)
- **Game Systems Layer**: Catan-specific implementations
- **Game-Specific Layer**: Catan game flow and coordination
- **Presentation Layer**: UI and visual representation

---

## PHASE 0: Project Setup & Documentation
**Status**: 🟢 Completed  
**Duration**: 1 day  
**Dependencies**: None  
**Completed**: 2024-12-19

### Goals
- Set up folder structure following new architecture
- Create comprehensive documentation
- Establish coding standards and patterns
- Prepare for incremental development

### Tasks
- [x] Create phase documentation
- [x] Create architecture documentation
- [x] Create coding standards document
- [x] Create progress tracking system
- [x] Create quick reference guide
- [x] Update README.md
- [x] Review and confirm plan (user confirmed)
- [ ] Create folder structure (will be done in Phase 1)

### Deliverables
- Complete project documentation
- Organized folder structure
- Development guidelines

---

## PHASE 1: Infrastructure Layer
**Status**: 🟡 In Progress  
**Duration**: 2-3 days  
**Dependencies**: Phase 0  
**Started**: 2024-12-19

### Goals
Build the foundation that all other systems depend on. Zero dependencies, maximum reusability.

### Tasks
1. **EventBus System**
   - [x] Create IEvent interface/base class
   - [x] Implement EventBus static class
   - [x] Add Subscribe<T>(Action<T>) method
   - [x] Add Unsubscribe<T>(Action<T>) method
   - [x] Add Publish<T>(T event) method
   - [x] Add Clear() method for cleanup
   - [x] Add optional debug logging
   - [x] Create test events for validation (TestEvent.cs)
   - [x] Write test script (InfrastructureTest.cs)

2. **ServiceLocator System**
   - [x] Implement ServiceLocator static class
   - [x] Add Register<T>(T service) method
   - [x] Add Get<T>() method
   - [x] Add TryGet<T>(out T service) method
   - [x] Add IsRegistered<T>() method
   - [x] Add Unregister<T>() method
   - [x] Add Clear() method
   - [x] Add validation (null checks, duplicate registration warnings)
   - [x] Write tests (in InfrastructureTest.cs)

3. **Logger System (Optional but Recommended)**
   - [x] Create ILogger interface
   - [x] Implement UnityLogger (wraps Debug.Log)
   - [x] Add log levels (Info, Warning, Error, Exception)
   - [x] Add context support for log messages
   - [ ] Add optional file logging (future - not needed now)

### Folder Structure
```
Assets/
└── Infrastructure/
    ├── EventBus/
    │   ├── EventBus.cs ✅
    │   ├── IEvent.cs ✅
    │   └── TestEvent.cs ✅
    ├── ServiceLocator/
    │   └── ServiceLocator.cs ✅
    ├── Logger/
    │   ├── ILogger.cs ✅
    │   └── UnityLogger.cs ✅
    └── Tests/
        └── InfrastructureTest.cs ✅
```

### Testing Requirements
- EventBus: Test subscribe/unsubscribe, publish, multiple subscribers, cleanup
- ServiceLocator: Test register/get/unregister, null handling, type safety
- Integration: Test EventBus + ServiceLocator working together

### Success Criteria
- ✅ EventBus can publish/subscribe events
- ✅ ServiceLocator can register/retrieve services
- ✅ No Unity dependencies in Infrastructure layer (except UnityLogger which wraps Unity's Debug)
- ✅ All systems can be tested independently
- ✅ Code is documented and follows standards
- ✅ Test script created for validation

---

## PHASE 2: Core Systems - Dice System
**Status**: 🟡 In Progress  
**Duration**: 2-3 days  
**Dependencies**: Phase 1  
**Started**: 2024-12-19

### Goals
Create a generic, reusable dice system that can work in any game requiring dice.

### Tasks
1. **Dice System Design**
   - [x] Define IDiceSystem interface
   - [x] Define DiceRollResult data class
   - [x] Define DiceConfig ScriptableObject
   - [x] Plan event structure (DiceRolledEvent)

2. **Dice System Implementation**
   - [x] Implement DiceSystem class
   - [x] Add Roll(int diceCount) method
   - [x] Add Roll(int diceCount, int sides) method
   - [x] Add Roll(DiceConfig config) method
   - [x] Add GetLastResult() method
   - [x] Add Reset() method
   - [x] Implement random number generation
   - [x] Publish DiceRolledEvent after each roll

3. **Dice Configuration**
   - [x] Create DiceConfig ScriptableObject
   - [x] Add fields: diceCount, sides, modifiers, minValue, maxValue
   - [ ] Create default config asset (can be created in Unity Editor)

4. **Events**
   - [x] Create DiceRolledEvent (in CoreSystems/Dice/Events/)
   - [x] Event contains: results, total, timestamp

5. **Testing**
   - [x] Create test script (DiceSystemTest.cs)
   - [x] Test various configurations
   - [x] Test event publishing
   - [x] Test last result tracking
   - [x] Test reset functionality

### Folder Structure
```
Assets/
└── CoreSystems/
    └── Dice/
        ├── IDiceSystem.cs
        ├── DiceSystem.cs
        ├── DiceRollResult.cs
        ├── DiceConfig.cs (ScriptableObject)
        └── Events/
            └── DiceRolledEvent.cs
```

### Design Decisions
- **Generic First**: No Catan-specific logic
- **Configurable**: Use ScriptableObjects for settings
- **Event-Driven**: All rolls publish events
- **Stateless**: System doesn't store game state

### Success Criteria
- ✅ Can roll any number of dice with any number of sides
- ✅ Results are properly randomized
- ✅ Events are published correctly
- ✅ System is completely game-agnostic
- ✅ Can be extracted to package without changes

---

## PHASE 3: Game Systems - Catan Dice System
**Status**: 🟢 Completed  
**Duration**: 1-2 days  
**Dependencies**: Phase 2  
**Completed**: 2024-12-19

### Goals
Wrap the generic DiceSystem with Catan-specific rules and logic.

### Tasks
1. **Catan Dice System**
   - [x] Create CatanDiceSystem class
   - [x] Compose DiceSystem (don't inherit)
   - [x] Implement Catan-specific roll logic (2 dice, 1-6 each)
   - [x] Handle special case: 7 = robber activation
   - [x] Publish CatanDiceRolledEvent with Catan context

2. **Catan Dice Events**
   - [x] Create CatanDiceRolledEvent
   - [x] Include: firstDie, secondDie, total, isRobber
   - [x] Inherit from IEvent

3. **Integration**
   - [x] Register CatanDiceSystem with ServiceLocator
   - [x] Create test script for validation
   - [ ] Replace existing DicesController (can be done when integrating with UI)
   - [ ] Connect to UI (can be done in Phase 14)

4. **Testing**
   - [x] Test normal rolls (2-12)
   - [x] Test robber activation (7)
   - [x] Test event flow
   - [x] Test ServiceLocator integration

### Folder Structure
```
Assets/
└── GameSystems/
    └── CatanDice/
        ├── CatanDiceSystem.cs
        └── Events/
            └── CatanDiceRolledEvent.cs
```

### Design Decisions
- **Composition**: CatanDiceSystem contains DiceSystem instance
- **Catan Rules**: Special handling for 7
- **Event Wrapping**: Publishes Catan-specific events
- **Service Registration**: Registers itself for easy access

### Success Criteria
- ✅ Wraps DiceSystem correctly
- ✅ Handles Catan dice rules (2 dice, 1-6)
- ✅ Correctly identifies robber (7)
- ✅ Publishes appropriate events
- ✅ Can be accessed via ServiceLocator
- ✅ Works with existing UI

---

## PHASE 4: Core Systems - Card System
**Status**: 🟢 Completed  
**Duration**: 3-4 days  
**Dependencies**: Phase 1  
**Completed**: 2024-12-19

### Goals
Create a generic card system for deck management, hand management, and card operations.

### Tasks
1. **Card System Design**
   - [x] Define ICard interface
   - [x] Define ICardSystem interface
   - [x] Define CardData structure
   - [x] Plan deck, hand, discard pile structure

2. **Card System Implementation**
   - [x] Implement CardSystem class
   - [x] Add CreateDeck() method
   - [x] Add ShuffleDeck() method (Fisher-Yates algorithm)
   - [x] Add DrawCard() method
   - [x] Add DrawCards(int count) method
   - [x] Add AddToHand() method
   - [x] Add DiscardCard() method
   - [x] Add GetHand() method
   - [x] Add GetDeckCount() method
   - [x] Add GetHandCount() method
   - [x] Add GetDiscardCount() method
   - [x] Add Reset() method

3. **Card Configuration**
   - [x] Create CardConfig ScriptableObject
   - [x] Define card types, counts, properties (CardTypeEntry)

4. **Events**
   - [x] Create CardDrawnEvent
   - [x] Create CardDiscardedEvent
   - [x] Create HandChangedEvent

5. **Testing**
   - [x] Test deck creation
   - [x] Test shuffling
   - [x] Test drawing (single and multiple)
   - [x] Test hand management
   - [x] Test discard
   - [x] Test event publishing
   - [x] Test reset

### Folder Structure
```
Assets/
└── CoreSystems/
    └── Cards/
        ├── ICard.cs
        ├── ICardSystem.cs
        ├── CardSystem.cs
        ├── CardData.cs
        ├── CardConfig.cs (ScriptableObject)
        └── Events/
            ├── CardDrawnEvent.cs
            ├── CardDiscardedEvent.cs
            └── HandChangedEvent.cs
```

### Success Criteria
- ✅ Generic card system works independently
- ✅ Can handle any card type
- ✅ Proper deck/hand/discard management
- ✅ Events published for all operations
- ✅ No game-specific logic

---

## PHASE 5: Game Systems - Catan Card System
**Status**: 🟢 Completed  
**Duration**: 2-3 days  
**Dependencies**: Phase 4  
**Completed**: 2024-12-19

### Goals
Implement Catan-specific cards (development cards) using the generic card system.

### Tasks
1. **Catan Card Types**
   - [x] Define CatanCardType enum (Knight, RoadBuilding, YearOfPlenty, Monopoly, VictoryPoint)
   - [x] Create CatanCardData class
   - [x] Define card properties (CanPlayImmediately, IsVictoryPoint)

2. **Catan Card System**
   - [x] Create CatanCardSystem class
   - [x] Compose CardSystem
   - [x] Implement development card deck creation (25 cards standard distribution)
   - [x] Implement card usage/activation
   - [x] Handle card mapping between generic and Catan cards

3. **Events**
   - [x] Create CatanCardDrawnEvent
   - [x] Create CatanCardUsedEvent

4. **Integration**
   - [x] Register with ServiceLocator
   - [x] Create test script
   - [ ] Replace existing CardController (can be done when integrating with UI)
   - [ ] Connect to UI (can be done in Phase 14)

### Success Criteria
- ✅ Development cards work correctly
- ✅ Card effects trigger properly
- ✅ Events published correctly
- ✅ UI integration works

---

## PHASE 6: Core Systems - Resource/Inventory System
**Status**: 🟢 Completed  
**Duration**: 2-3 days  
**Dependencies**: Phase 1, Phase 4  
**Completed**: 2024-12-19

### Goals
Create a generic resource/inventory management system.

### Tasks
1. **Resource System Design**
   - [x] Define IResourceSystem interface
   - [x] Define ResourceInventory class
   - [x] Define ResourceType as generic (string-based)
   - [x] Plan CardSystem integration (hybrid approach)
   - [x] Plan add/remove/query operations

2. **Resource System Implementation**
   - [x] Implement ResourceSystem class
   - [x] Use CardSystem internally (one per resource type)
   - [x] Add AddResource() method (adds cards to hand)
   - [x] Add RemoveResource() method (removes cards from hand)
   - [x] Add HasResource() method (checks card count)
   - [x] Add GetResourceCount() method (returns hand count)
   - [x] Add TransferResource() method (transfers cards between inventories)
   - [x] Add CreateResourceDeck() method (creates deck of identical cards)
   - [x] Add DrawFromDeck() method (draws from bank/supply)
   - [x] Add GetDeckCount() method (checks deck availability)

3. **Events**
   - [x] Create ResourceAddedEvent
   - [x] Create ResourceRemovedEvent
   - [x] Create ResourceChangedEvent

4. **Testing**
   - [x] Test resource operations
   - [x] Test validation
   - [x] Test events
   - [x] Test deck operations
   - [x] Test transfer operations

### Success Criteria
- ✅ Generic resource system works
- ✅ Uses CardSystem internally for card management
- ✅ Simple interface for resource operations
- ✅ Can handle any resource type
- ✅ Proper validation (no negatives)
- ✅ Events published correctly

---

## PHASE 7: Game Systems - Catan Resource System
**Status**: ⚪ Not Started  
**Duration**: 2-3 days  
**Dependencies**: Phase 6

### Goals
Implement Catan-specific resources and distribution.

### Tasks
1. **Catan Resources**
   - [ ] Use existing ResourceType enum (Wood, Sheep, Brick, Wheat, Ore)
   - [ ] Create CatanResourceInventory
   - [ ] Implement Catan resource deck creation (5 decks, ~18 cards each)
   - [ ] Implement Catan resource distribution rules

2. **Catan Resource System**
   - [ ] Create CatanResourceSystem
   - [ ] Compose ResourceSystem (which uses CardSystem internally)
   - [ ] Implement dice-based distribution
   - [ ] Handle robber blocking
   - [ ] Connect to bank resource supply

3. **Integration**
   - [ ] Connect to CatanDiceSystem
   - [ ] Connect to MapGenerator
   - [ ] Connect to UI

### Success Criteria
- ✅ Resource distribution works
- ✅ Dice-based collection works
- ✅ Robber blocking works
- ✅ Bank resource supply managed correctly
- ✅ UI updates correctly

---

## PHASE 8: Game Systems - Terrain Generation Refactoring
**Status**: ⚪ Not Started  
**Duration**: 2-3 days  
**Dependencies**: Phase 1

### Goals
Refactor existing MapGenerator to follow new architecture and integrate with game systems.

### Tasks
1. **Terrain System Design**
   - [ ] Analyze existing MapGenerator.cs
   - [ ] Define ITerrainSystem interface
   - [ ] Define TerrainData class
   - [ ] Plan integration with ResourceSystem

2. **Terrain System Implementation**
   - [ ] Create TerrainSystem class
   - [ ] Refactor map generation logic
   - [ ] Implement terrain placement
   - [ ] Implement dice number assignment
   - [ ] Implement resource type assignment
   - [ ] Add validation rules (6 and 8 not adjacent, etc.)

3. **Integration**
   - [ ] Connect to ResourceSystem (for resource distribution)
   - [ ] Connect to CatanDiceSystem (for dice numbers)
   - [ ] Maintain compatibility with existing Field system
   - [ ] Connect to UI

4. **Events**
   - [ ] Create TerrainGeneratedEvent
   - [ ] Create TerrainPlacedEvent

### Success Criteria
- ✅ Map generation works correctly
- ✅ Follows new architecture patterns
- ✅ Integrates with other systems
- ✅ Validation rules enforced
- ✅ Events published correctly
- ✅ Compatible with existing Field/MapGenerator code

---

## PHASE 9: Game Systems - Building System
**Status**: ⚪ Not Started  
**Duration**: 4-5 days  
**Dependencies**: Phase 7

### Goals
Implement Catan building placement and validation.

### Tasks
1. **Building System Design**
   - [ ] Define BuildingType enum
   - [ ] Define BuildingData class
   - [ ] Define validation rules structure

2. **Building System Implementation**
   - [ ] Create BuildingSystem class
   - [ ] Implement placement validation
   - [ ] Implement distance rules
   - [ ] Implement road connectivity
   - [ ] Implement cost validation
   - [ ] Add PlaceBuilding() method
   - [ ] Add CanPlaceBuilding() method

3. **Events**
   - [ ] Create BuildingPlacedEvent
   - [ ] Create BuildingValidationRequestEvent
   - [ ] Create BuildingValidationResponseEvent

4. **Integration**
   - [ ] Connect to ResourceSystem (costs)
   - [ ] Connect to existing placeholder system
   - [ ] Connect to UI

### Success Criteria
- ✅ Building placement works
- ✅ All validation rules enforced
- ✅ Costs deducted correctly
- ✅ Events published correctly

---

## PHASE 10: Game Systems - Victory System
**Status**: ⚪ Not Started  
**Duration**: 2-3 days  
**Dependencies**: Phase 9

### Goals
Implement Catan robber mechanics.

### Tasks
1. **Robber System**
   - [ ] Create RobberSystem class
   - [ ] Implement robber placement
   - [ ] Implement resource stealing
   - [ ] Implement resource discard (7+ resources)

2. **Events**
   - [ ] Create RobberPlacedEvent
   - [ ] Create ResourceStolenEvent
   - [ ] Create ResourceDiscardEvent

3. **Integration**
   - [ ] Connect to CatanDiceSystem (7 trigger)
   - [ ] Connect to ResourceSystem
   - [ ] Connect to UI

### Success Criteria
- ✅ Robber placement works
- ✅ Resource stealing works
- ✅ Discard mechanic works
- ✅ UI integration works

---

## PHASE 11: Game-Specific - Game Flow
**Status**: ⚪ Not Started  
**Duration**: 4-5 days  
**Dependencies**: Phase 10

### Goals
Implement victory point tracking and win condition.

### Tasks
1. **Victory System**
   - [ ] Create VictorySystem class
   - [ ] Implement point calculation
   - [ ] Track points from buildings
   - [ ] Track special cards (longest road, largest army)
   - [ ] Implement win detection (10 points)

2. **Events**
   - [ ] Create VictoryPointGainedEvent
   - [ ] Create GameWonEvent

3. **Integration**
   - [ ] Connect to BuildingSystem
   - [ ] Connect to CardSystem
   - [ ] Connect to GameManager

### Success Criteria
- ✅ Points calculated correctly
- ✅ Win condition detected
- ✅ Events published correctly

---

## PHASE 11: Game-Specific - Game Flow
**Status**: ⚪ Not Started  
**Duration**: 4-5 days  
**Dependencies**: Phase 10

### Goals
Orchestrate all systems into a complete game flow for playable prototype.

### Tasks
1. **GameManager**
   - [ ] Create GameManager singleton
   - [ ] Implement state machine
   - [ ] Manage game phases (setup, play, end)
   - [ ] Coordinate system initialization
   - [ ] Handle game start/end

2. **PlayerManager**
   - [ ] Create PlayerManager
   - [ ] Implement player creation
   - [ ] Implement turn order
   - [ ] Track active player

3. **Game State**
   - [ ] Create GameState class
   - [ ] Track current phase
   - [ ] Track turn number
   - [ ] Track player states

4. **Integration**
   - [ ] Wire all systems together (Resources, Building, Victory, Terrain)
   - [ ] Connect events
   - [ ] Test complete flow
   - [ ] Ensure playable prototype works end-to-end

### Success Criteria
- ✅ Game can start and end
- ✅ All phases work correctly
- ✅ Turn order works
- ✅ All systems integrated
- ✅ Basic playable prototype functional

---

## PHASE 12: Presentation - UI Integration
**Status**: ⚪ Not Started  
**Duration**: 3-4 days  
**Dependencies**: Phase 11

### Goals
Connect UI to game systems via events for playable prototype.

### Tasks
1. **UIManager**
   - [ ] Create UIManager
   - [ ] Subscribe to game events
   - [ ] Update UI based on events
   - [ ] Publish user action events

2. **UI Components (Priority for Prototype)**
   - [ ] Update dice UI
   - [ ] Update resource UI
   - [ ] Update building UI
   - [ ] Update victory point UI
   - [ ] Update terrain/map UI
   - [ ] (Defer: card UI, trading UI to later phases)

3. **Integration**
   - [ ] Connect all UI to systems
   - [ ] Test user interactions
   - [ ] Test visual feedback

### Success Criteria
- ✅ All UI updates correctly
- ✅ User actions trigger game events
- ✅ Visual feedback works
- ✅ Playable prototype complete

---

## PHASE 13: Polish & Testing
**Status**: ⚪ Not Started  
**Duration**: 3-5 days  
**Dependencies**: Phase 12

### Goals
Polish the game and ensure everything works correctly for playable prototype.

### Tasks
1. **Testing**
   - [ ] Test all game scenarios
   - [ ] Test edge cases
   - [ ] Test error handling
   - [ ] Performance testing

2. **Polish**
   - [ ] Visual polish
   - [ ] Sound effects (optional)
   - [ ] Animations (optional)
   - [ ] UI/UX improvements

3. **Documentation**
   - [ ] Code documentation
   - [ ] Usage guides
   - [ ] Package extraction guides

### Success Criteria
- ✅ Game is fully playable
- ✅ All systems work correctly
- ✅ Code is clean and documented
- ✅ Ready for package extraction

---

## PHASE 14: Game Systems - Robber System
**Status**: ⚪ Not Started  
**Duration**: 2-3 days  
**Dependencies**: Phase 7, Phase 9

### Goals
Implement Catan robber mechanics (deferred from initial prototype).

### Tasks
1. **Robber System**
   - [ ] Create RobberSystem class
   - [ ] Implement robber placement
   - [ ] Implement resource stealing
   - [ ] Implement resource discard (7+ resources)

2. **Events**
   - [ ] Create RobberPlacedEvent
   - [ ] Create ResourceStolenEvent
   - [ ] Create ResourceDiscardEvent

3. **Integration**
   - [ ] Connect to CatanDiceSystem (7 trigger)
   - [ ] Connect to ResourceSystem
   - [ ] Connect to UI

### Success Criteria
- ✅ Robber placement works
- ✅ Resource stealing works
- ✅ Discard mechanic works
- ✅ UI integration works

---

## PHASE 15: Core Systems - Trading System
**Status**: ⚪ Not Started  
**Duration**: 3-4 days  
**Dependencies**: Phase 1

### Goals
Create a generic trading system for resource/item exchange (deferred from initial prototype).

### Tasks
1. **Trading System Design**
   - [ ] Define ITradingSystem interface
   - [ ] Define TradeOffer structure
   - [ ] Define TradeResult structure
   - [ ] Plan validation system

2. **Trading System Implementation**
   - [ ] Implement TradingSystem class
   - [ ] Add CreateOffer() method
   - [ ] Add AcceptOffer() method
   - [ ] Add RejectOffer() method
   - [ ] Add ValidateOffer() method
   - [ ] Add ExecuteTrade() method

3. **Events**
   - [ ] Create TradeOfferCreatedEvent
   - [ ] Create TradeAcceptedEvent
   - [ ] Create TradeRejectedEvent

4. **Testing**
   - [ ] Test offer creation
   - [ ] Test validation
   - [ ] Test trade execution
   - [ ] Test event flow

### Success Criteria
- ✅ Generic trading system works
- ✅ Can trade any resource/item type
- ✅ Proper validation
- ✅ Events published correctly

---

## PHASE 16: Game Systems - Catan Trading System
**Status**: ⚪ Not Started  
**Duration**: 2-3 days  
**Dependencies**: Phase 15

### Goals
Implement Catan-specific trading (player-to-player, ports) (deferred from initial prototype).

### Tasks
1. **Catan Trading Rules**
   - [ ] Implement port trading (3:1, 2:1 ratios)
   - [ ] Implement player-to-player trading
   - [ ] Add trading validation rules

2. **Catan Trading System**
   - [ ] Create CatanTradingSystem
   - [ ] Compose TradingSystem
   - [ ] Add port trading methods
   - [ ] Add player trading methods

3. **Integration**
   - [ ] Connect to ResourceSystem
   - [ ] Connect to UI

### Success Criteria
- ✅ Port trading works
- ✅ Player trading works
- ✅ Validation correct
- ✅ UI integration works

### Goals
Polish the game and ensure everything works correctly.

### Tasks
1. **Testing**
   - [ ] Test all game scenarios
   - [ ] Test edge cases
   - [ ] Test error handling
   - [ ] Performance testing

2. **Polish**
   - [ ] Visual polish
   - [ ] Sound effects (optional)
   - [ ] Animations (optional)
   - [ ] UI/UX improvements

3. **Documentation**
   - [ ] Code documentation
   - [ ] Usage guides
   - [ ] Package extraction guides

### Success Criteria
- ✅ Game is fully playable
- ✅ All systems work correctly
- ✅ Code is clean and documented
- ✅ Ready for package extraction

---

## Progress Tracking

### Current Phase
**Phase 0: Project Setup & Documentation** 🟡 In Progress

### Completed Phases
None yet

### Next Phase
**Phase 1: Infrastructure Layer** ⚪ Not Started

### Blockers
None currently

### Notes
- Starting fresh with new architecture
- Will refactor existing code as we progress
- Focus on reusability from the start

