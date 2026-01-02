# Catan Card System

## Overview
The Catan Card System wraps the generic Card System with Catan-specific development card rules. It manages the standard Catan development card deck (25 cards: 14 Knights, 2 Road Building, 2 Year of Plenty, 2 Monopoly, 5 Victory Points).

**Package**: Part of `com.yourcompany.catansystems`  
**Dependencies**: Core Systems (Card System), Infrastructure

---

## What It Does

The Catan Card System:
- Creates the standard Catan development card deck (25 cards)
- Manages Catan-specific card types (Knight, Road Building, Year of Plenty, Monopoly, Victory Point)
- Handles card drawing and usage
- Publishes Catan-specific events
- Provides easy access via ServiceLocator

---

## How to Use

### Basic Usage

**Create Deck and Draw:**
```csharp
// Create Catan card system (auto-registers with ServiceLocator)
CatanCardSystem catanCards = new CatanCardSystem();

// Create the standard development card deck
catanCards.CreateDevelopmentCardDeck();

// Draw a card
CatanCardDrawnEvent drawEvent = catanCards.DrawDevelopmentCard();

if (drawEvent != null)
{
    Debug.Log($"Drew: {drawEvent.Card.CardName}");
    
    // Add to hand
    catanCards.AddToHand(drawEvent.Card);
}
```

**Use a Card:**
```csharp
// Get hand
List<CatanCardData> hand = catanCards.GetHand();

// Use a card
if (hand.Count > 0)
{
    catanCards.UseCard(hand[0], playerID: 1);
    // CatanCardUsedEvent is automatically published
}
```

### Using ServiceLocator

**Get Catan Card System:**
```csharp
// Get from ServiceLocator (must be registered first)
CatanCardSystem catanCards = ServiceLocator.Get<CatanCardSystem>();

if (catanCards != null)
{
    catanCards.CreateDevelopmentCardDeck();
    CatanCardDrawnEvent drawEvent = catanCards.DrawDevelopmentCard();
}
```

**Register Manually:**
```csharp
// Create without auto-registration
CatanCardSystem catanCards = new CatanCardSystem(autoRegister: false);

// Register manually
ServiceLocator.Register<CatanCardSystem>(catanCards);
```

### Listening to Events

**Subscribe to Catan Card Events:**
```csharp
private void OnEnable()
{
    EventBus.Subscribe<CatanCardDrawnEvent>(OnCatanCardDrawn);
    EventBus.Subscribe<CatanCardUsedEvent>(OnCatanCardUsed);
}

private void OnDisable()
{
    EventBus.Unsubscribe<CatanCardDrawnEvent>(OnCatanCardDrawn);
    EventBus.Unsubscribe<CatanCardUsedEvent>(OnCatanCardUsed);
}

private void OnCatanCardDrawn(CatanCardDrawnEvent evt)
{
    Debug.Log($"Catan card drawn: {evt.Card.CardName}");
    
    // Check if it's a Knight (can be played immediately)
    if (evt.Card.CanPlayImmediately)
    {
        Debug.Log("Knight card can be played immediately!");
    }
    
    // Check if it's a Victory Point (hidden)
    if (evt.Card.IsVictoryPoint)
    {
        Debug.Log("Victory Point card (hidden until end of game)");
    }
}

private void OnCatanCardUsed(CatanCardUsedEvent evt)
{
    Debug.Log($"Player {evt.PlayerID} used {evt.Card.CardName}");
    
    // Handle card effects based on type
    switch (evt.Card.CatanType)
    {
        case CatanCardType.Knight:
            // Move robber and steal resource
            break;
        case CatanCardType.RoadBuilding:
            // Place 2 roads for free
            break;
        case CatanCardType.YearOfPlenty:
            // Take any 2 resources
            break;
        case CatanCardType.Monopoly:
            // Take all of one resource type from all players
            break;
    }
}
```

---

## Catan Card Types

### Knight (14 cards)
- **Effect**: Move robber and steal one resource from adjacent player
- **Can Play Immediately**: Yes (can be played on the turn it's drawn)
- **Victory Point**: No

### Road Building (2 cards)
- **Effect**: Place 2 roads for free (without spending resources)
- **Can Play Immediately**: No
- **Victory Point**: No

### Year of Plenty (2 cards)
- **Effect**: Take any 2 resources from the bank
- **Can Play Immediately**: No
- **Victory Point**: No

### Monopoly (2 cards)
- **Effect**: Take all of one resource type from all players
- **Can Play Immediately**: No
- **Victory Point**: No

### Victory Point (5 cards)
- **Effect**: Worth 1 victory point (hidden until end of game)
- **Can Play Immediately**: No
- **Victory Point**: Yes

---

## CatanCardData Properties

- `CardID`: Unique identifier
- `CardType`: String type (e.g., "Knight")
- `CardName`: Display name (e.g., "Knight")
- `CatanType`: Catan-specific enum type
- `CanPlayImmediately`: True for Knight cards
- `IsVictoryPoint`: True for Victory Point cards

---

## How to Modify/Extend

### Creating Custom Catan Card Variants

**Option 1: Extend CatanCardData**
```csharp
public class CustomCatanCardData : CatanCardData
{
    public int CustomProperty { get; private set; }
    
    public CustomCatanCardData(CatanCardType type, int customProp) 
        : base(type)
    {
        CustomProperty = customProp;
    }
}
```

**Option 2: Wrap CatanCardSystem**
```csharp
public class EnhancedCatanCardSystem
{
    private CatanCardSystem catanCards;
    
    public EnhancedCatanCardSystem()
    {
        catanCards = new CatanCardSystem();
    }
    
    public CatanCardDrawnEvent DrawWithHistory()
    {
        CatanCardDrawnEvent drawEvent = catanCards.DrawDevelopmentCard();
        // Track in history
        return drawEvent;
    }
}
```

### Custom Event Handling

**Create Extended Event:**
```csharp
public class ExtendedCatanCardUsedEvent : CatanCardUsedEvent
{
    public bool WasSuccessful { get; }
    
    public ExtendedCatanCardUsedEvent(CatanCardData card, int playerID, bool success) 
        : base(card, playerID)
    {
        WasSuccessful = success;
    }
}
```

---

## Integration with Existing Code

### Replacing CardController

**Old Code (MareGameplay.CardController):**
```csharp
// Old way
cardController.Select();
```

**New Code:**
```csharp
// New way
CatanCardSystem catanCards = ServiceLocator.Get<CatanCardSystem>();
List<CatanCardData> hand = catanCards.GetHand();

if (hand.Count > 0)
{
    catanCards.UseCard(hand[0], playerID: currentPlayerID);
}
```

### UI Integration

**Update UI on Card Draw:**
```csharp
public class DevelopmentCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI handCountText;
    [SerializeField] private Button drawCardButton;
    
    private CatanCardSystem catanCards;
    
    private void OnEnable()
    {
        EventBus.Subscribe<CatanCardDrawnEvent>(OnCardDrawn);
        EventBus.Subscribe<HandChangedEvent>(OnHandChanged);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe<CatanCardDrawnEvent>(OnCardDrawn);
        EventBus.Unsubscribe<HandChangedEvent>(OnHandChanged);
    }
    
    private void OnCardDrawn(CatanCardDrawnEvent evt)
    {
        Debug.Log($"Drew: {evt.Card.CardName}");
        UpdateHandDisplay();
    }
    
    private void OnHandChanged(HandChangedEvent evt)
    {
        UpdateHandDisplay();
    }
    
    private void UpdateHandDisplay()
    {
        if (catanCards == null)
        {
            catanCards = ServiceLocator.Get<CatanCardSystem>();
        }
        
        int handCount = catanCards.GetHandCount();
        handCountText.text = $"Hand: {handCount}";
    }
    
    public void OnDrawCardClicked()
    {
        if (catanCards == null)
        {
            catanCards = ServiceLocator.Get<CatanCardSystem>();
        }
        
        CatanCardDrawnEvent drawEvent = catanCards.DrawDevelopmentCard();
        if (drawEvent != null)
        {
            catanCards.AddToHand(drawEvent.Card);
        }
    }
}
```

---

## Best Practices

1. **Use ServiceLocator**: Register and retrieve via ServiceLocator for easy access
2. **Subscribe to Events**: Use events for decoupled card handling
3. **Check Card Properties**: Use `CanPlayImmediately` and `IsVictoryPoint` for game logic
4. **Handle Card Effects**: Subscribe to `CatanCardUsedEvent` to handle card effects
5. **Unregister**: Call `Unregister()` when disposing of the system

---

## Testing

Use the `CatanCardSystemTest` script to validate functionality:
1. Add `CatanCardSystemTest` component to a GameObject
2. Run the scene
3. Check console for test results

All tests should pass:
- ✅ Create Deck
- ✅ Draw Card
- ✅ Hand Management
- ✅ Use Card
- ✅ Event Publishing
- ✅ Service Locator
- ✅ Reset

---

## Catan Rules Implementation

### Development Card Rules
- **Deck Size**: 25 cards total
- **Distribution**: 14 Knights, 2 Road Building, 2 Year of Plenty, 2 Monopoly, 5 Victory Points
- **Knight Cards**: Can be played immediately after drawing
- **Victory Points**: Hidden until end of game
- **Other Cards**: Must wait until next turn to play

### Card Usage
- Cards are drawn from the deck
- Cards are added to hand
- Cards are used/activated
- Used cards are discarded

---

## Version History

- **1.0.0** (2024-12-19): Initial release
  - Catan development card system
  - Standard deck distribution
  - Card drawing and usage
  - Event publishing
  - ServiceLocator integration

---

## Support

For issues or questions, refer to the main project documentation or create an issue in the repository.

