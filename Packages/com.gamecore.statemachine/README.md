# com.gamecore.statemachine

Generic state machine used by the turn system, robber flow, and any UI that has distinct modes.

## What it does

Manages a current state, routes `Enter` / `Execute` / `Exit` calls, and publishes events on transitions. Knows nothing about Catan — the context type `TContext` is injected by the caller.

## Key types

| Type | Role |
|---|---|
| `IState<TContext>` | Interface every state implements |
| `StateMachine<TContext>` | Holds current state, drives transitions and updates |
| `StateTransition<TContext>` | Data object pairing a From state, a To state, and a condition |

## Events published

`StateEnteredEvent<T>`, `StateExitedEvent<T>`, `TransitionFiredEvent<T>`

## Usage pattern

```csharp
var sm = new StateMachine<CatanTurnContext>(context);
sm.Transition(new RollDiceState());   // Enter called
sm.Update();                           // Execute called each frame / tick
sm.Transition(new TradingState());    // Exit on old, Enter on new
```

## Dependencies

`com.gamecore.events`
