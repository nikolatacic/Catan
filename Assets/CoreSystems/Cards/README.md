# Card System Package

## Overview
The Card System provides a generic, reusable card management system for deck creation, shuffling, drawing, hand management, and discard operations. It's completely game-agnostic and can be configured for various card types and scenarios.

**Package Name**: `com.yourcompany.coresystems` (Cards module)  
**Version**: 1.0.0  
**Dependencies**: Infrastructure package (`com.yourcompany.infrastructure`)

---

## What It Does

The Card System allows you to:
- Create decks from configuration
- Shuffle decks
- Draw cards (single or multiple)
- Manage hands
- Discard cards
- Track deck, hand, and discard pile counts
- Receive events for all card operations

---

## How to Use

### Basic Usage

**Create Deck and Draw:**
```csharp
// Create card system
ICardSystem cardSystem = new CardSystem();

// Create deck from config
CardConfig config = Resources.Load<CardConfig>("CardConfigs/DevelopmentCards");
cardSystem.CreateDeck(config);

// Shuffle
cardSystem.ShuffleDeck();

// Draw a card
ICard card = cardSystem.DrawCard();
// CardDrawnEvent is automatically published

// Add to hand
cardSystem.AddToHand(card);
// HandChangedEvent is automatically published
```

**Draw Multiple Cards:**
```csharp
// Draw 3 cards at once
List<ICard> cards = cardSystem.DrawCards(3);
foreach (ICard card in cards)
{
    cardSystem.AddToHand(card);
}
```

**Discard Cards:**
```csharp
// Discard a card from hand
cardSystem.DiscardCard(card);
// CardDiscardedEvent and HandChangedEvent are automatically published
```

### Listening to Events

**Subscribe to Card Events:**
```csharp
private void OnEnable()
{
    EventBus.Subscribe<CardDrawnEvent>(OnCardDrawn);
    EventBus.Subscribe<CardDiscardedEvent>(OnCardDiscarded);
    EventBus.Subscribe<HandChangedEvent>(OnHandChanged);
}

private void OnDisable()
{
    EventBus.Unsubscribe<CardDrawnEvent>(OnCardDrawn);
    EventBus.Unsubscribe<CardDiscardedEvent>(OnCardDiscarded);
    EventBus.Unsubscribe<HandChangedEvent>(OnHandChanged);
}

private void OnCardDrawn(CardDrawnEvent evt)
{
    Debug.Log($"Card drawn: {evt.Card.CardName}");
}

private void OnCardDiscarded(CardDiscardedEvent evt)
{
    Debug.Log($"Card discarded: {evt.Card.CardName}");
}

private void OnHandChanged(HandChangedEvent evt)
{
    Debug.Log($"Hand changed: {evt.HandCount} cards in hand");
}
```

### Using ServiceLocator

**Register Card System:**
```csharp
// In initialization
ICardSystem cardSystem = new CardSystem();
ServiceLocator.Register<ICardSystem>(cardSystem);
```

**Get Card System:**
```csharp
// Anywhere in your code
ICardSystem cardSystem = ServiceLocator.Get<ICardSystem>();
cardSystem.CreateDeck(config);
```

---

## ScriptableObject Configuration

### Creating a CardConfig

1. **In Unity Editor**: Right-click in Project window
2. **Select**: `Create > Core Systems > Card Config`
3. **Name it**: e.g., "DevelopmentCards", "ActionCards", "ResourceCards"
4. **Configure**:
   - Add entries to the Card Types list
   - For each entry:
     - **Card Type**: Type identifier (e.g., "Knight", "RoadBuilding")
     - **Card Name**: Display name (optional, defaults to card type)
     - **Count**: Number of this card type in the deck

### Example Configurations

**Development Cards (Catan):**
- Knight: 14 cards
- Road Building: 2 cards
- Year of Plenty: 2 cards
- Monopoly: 2 cards
- Victory Point: 5 cards

**Standard Playing Cards:**
- Ace: 4 cards (one per suit)
- King: 4 cards
- Queen: 4 cards
- ... (and so on)

### Using Config in Code

**Load from Resources:**
```csharp
CardConfig config = Resources.Load<CardConfig>("CardConfigs/DevelopmentCards");
cardSystem.CreateDeck(config);
```

**Reference in Inspector:**
```csharp
public class MyGameManager : MonoBehaviour
{
    [SerializeField] private CardConfig cardConfig;
    
    private void Start()
    {
        ICardSystem cardSystem = ServiceLocator.Get<ICardSystem>();
        cardSystem.CreateDeck(cardConfig);
        cardSystem.ShuffleDeck();
    }
}
```

**Create Programmatically:**
```csharp
CardConfig config = ScriptableObject.CreateInstance<CardConfig>();
CardConfig.CardTypeEntry entry = new CardConfig.CardTypeEntry
{
    cardType = "Knight",
    cardName = "Knight",
    count = 14
};
config.CardTypes = new List<CardConfig.CardTypeEntry> { entry };

cardSystem.CreateDeck(config);
```

---

## How to Modify/Upgrade/Extend

### Creating Custom Card Types

**Option 1: Extend CardData**
```csharp
public class CustomCardData : CardData
{
    public int PowerLevel { get; private set; }
    
    public CustomCardData(string cardType, string cardName, int powerLevel) 
        : base(cardType, cardName)
    {
        PowerLevel = powerLevel;
    }
}
```

**Option 2: Implement ICard Directly**
```csharp
public class MyCustomCard : ICard
{
    public int CardID { get; private set; }
    public string CardType { get; private set; }
    public string CardName { get; private set; }
    public int CustomProperty { get; private set; }
    
    // Implement interface
}
```

### Extending CardSystem

**Option 1: Wrap CardSystem (Recommended)**
```csharp
public class EnhancedCardSystem
{
    private ICardSystem cardSystem;
    
    public EnhancedCardSystem()
    {
        cardSystem = new CardSystem();
    }
    
    public ICard DrawCardWithHistory()
    {
        ICard card = cardSystem.DrawCard();
        // Track in history
        return card;
    }
}
```

**Option 2: Implement ICardSystem**
```csharp
public class CustomCardSystem : ICardSystem
{
    // Implement all interface methods
    public void CreateDeck(CardConfig config) { /* Custom logic */ }
    public void ShuffleDeck() { /* Custom logic */ }
    // ... etc
}
```

### Creating Custom Events

**Extend Existing Events:**
```csharp
public class ExtendedCardDrawnEvent : CardDrawnEvent
{
    public bool IsSpecialCard { get; }
    
    public ExtendedCardDrawnEvent(ICard card, bool isSpecial) 
        : base(card)
    {
        IsSpecialCard = isSpecial;
    }
}
```

---

## Advanced Usage

### Multiple Decks

```csharp
// Create multiple card systems for different decks
ICardSystem mainDeck = new CardSystem();
ICardSystem discardDeck = new CardSystem();

mainDeck.CreateDeck(mainConfig);
discardDeck.CreateDeck(discardConfig);
```

### Deck Reshuffling

```csharp
// When deck is empty, reshuffle discard pile back into deck
if (cardSystem.GetDeckCount() == 0 && cardSystem.GetDiscardCount() > 0)
{
    List<ICard> discardPile = cardSystem.GetDiscard(); // Would need to add this method
    foreach (ICard card in discardPile)
    {
        cardSystem.AddToDeck(card); // Would need to add this method
    }
    cardSystem.ShuffleDeck();
}
```

### Card Filtering

```csharp
// Get cards of specific type from hand
List<ICard> hand = cardSystem.GetHand();
List<ICard> knights = hand.Where(c => c.CardType == "Knight").ToList();
```

---

## Best Practices

1. **Use Interface**: Always use `ICardSystem` interface, not `CardSystem` class directly
2. **Register with ServiceLocator**: Register card system for easy access
3. **Subscribe to Events**: Use events for decoupled card handling
4. **Shuffle After Creation**: Always shuffle deck after creating it
5. **Validate Before Drawing**: Check deck count before drawing cards
6. **Use Configs**: Create ScriptableObject configs for different deck types

---

## Testing

Use the `CardSystemTest` script to validate functionality:
1. Add `CardSystemTest` component to a GameObject
2. Run the scene
3. Check console for test results

All tests should pass:
- ✅ Create Deck
- ✅ Shuffle
- ✅ Draw Card
- ✅ Draw Multiple
- ✅ Hand Management
- ✅ Discard
- ✅ Event Publishing
- ✅ Reset

---

## Package Extraction

When extracting to a package:
1. Copy `CoreSystems/Cards/` folder to package structure
2. Create `package.json` with Infrastructure dependency
3. Update namespaces if needed
4. Test package in isolation
5. Ensure Infrastructure package is available

---

## Version History

- **1.0.0** (2024-12-19): Initial release
  - Generic card system
  - Deck, hand, discard management
  - Event publishing
  - ScriptableObject configuration

---

## Support

For issues or questions, refer to the main project documentation or create an issue in the repository.

