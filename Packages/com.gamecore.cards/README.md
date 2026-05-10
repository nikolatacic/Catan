# com.gamecore.cards

Generic deck and hand system. Cards are self-contained — each implements its own `IsPlayable` check and `OnPlay` effect via an `IGameContext`.

## What it does

`CardDeck<T>` stores an ordered list of cards and supports draw, shuffle, and add-to-top/bottom. `CardHand<T>` is a player's held cards — it guards `IsPlayable` before calling `OnPlay` and fires events for UI updates.

## Key types

| Type | Role |
|---|---|
| `ICard` | Contract: `CardId`, `DisplayName`, `IsPlayable`, `OnPlay` |
| `IGameContext` | Passed into `IsPlayable` and `OnPlay` so cards can read game state |
| `CardDeck<T>` | Ordered deck with shuffle and draw |
| `CardHand<T>` | Player's held cards; gates play through `IsPlayable` |

## Events published

`CardDrawnEvent<T>`, `CardPlayedEvent<T>`, `CardDiscardedEvent<T>`, `DeckEmptyEvent`

## Dependencies

`com.gamecore.events`, `com.gamecore.player`

## Extension point

Catan's `DevelopmentCard` subclasses (`KnightCard`, `VictoryPointCard`, etc.) live in `Assets/Catan/Cards/`. `IGameContext` is implemented by `CatanTurnManager` which exposes `ActivePlayer`, `RobberSystem`, and other Catan-specific handles cards need at play time.
