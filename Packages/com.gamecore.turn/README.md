# com.gamecore.turn

Orchestrates whose turn it is and what phase is active. Other systems react to phase events rather than being called directly by the turn manager.

## What it does

`TurnManager` holds a list of `ITurnActor` (players), tracks the current actor and phase, and exposes `NextTurn` / `AdvancePhase` / `SkipActor`. It publishes events so any system can react to turn changes without needing a direct reference to the manager.

`ITurnActor` is the minimal interface a player must implement to participate in turns — just `Id` and `CanAct`. Catan's `CatanPlayer` implements this alongside `IPlayer`.

## Key types

| Type | Role |
|---|---|
| `ITurnActor` | Anything that can take a turn |
| `TurnPhase` | Base enum (Start / Main / End); overridden by Catan |
| `TurnManager` | MonoBehaviour; owns actor list and phase state |

## Events published

`TurnStartedEvent`, `TurnEndedEvent`, `PhaseChangedEvent`, `ActorSkippedEvent`

## Dependencies

`com.gamecore.events`, `com.gamecore.statemachine`, `com.gamecore.player`

## Extension point

`CatanTurnManager` subclasses `TurnManager` and adds `CatanTurnPhase` (RollDice / Robber / Trading / Building / EndTurn), `DiceManager`, and `RobberSystem`. The base class logic is intentionally thin so the Catan subclass controls the full phase flow.
