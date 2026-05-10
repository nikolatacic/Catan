# com.gamecore.events

Foundation package. All other packages depend on this one; it depends on nothing.

## What it does

Provides two ways for systems to communicate without holding direct references to each other:

**EventBus** — static, in-memory pub/sub for pure C# code. Any class can publish or subscribe anywhere without a MonoBehaviour context.

**GameEventChannel\<T\>** — ScriptableObject-based channel for inspector-wired Unity scenes. Drop a channel asset into a field; the sender calls `Raise()`, receivers register via `Register()` in `OnEnable`.

## Key types

| Type | Role |
|---|---|
| `IGameEvent` | Marker interface every event struct must implement |
| `EventBus` | Static subscribe / unsubscribe / publish |
| `GameEventChannel<T>` | ScriptableObject channel for inspector wiring |
| `IEventListener<T>` | Optional interface for MonoBehaviours that want a typed callback |

## Usage pattern

```csharp
// Publisher (pure C#)
EventBus.Publish(new DiceRolledEvent { D1 = 3, D2 = 4, Total = 7 });

// Subscriber (MonoBehaviour)
void OnEnable()  => EventBus.Subscribe<DiceRolledEvent>(OnDiceRolled);
void OnDisable() => EventBus.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
```

Always unsubscribe in `OnDisable` / `OnDestroy` to avoid leaked handlers.
