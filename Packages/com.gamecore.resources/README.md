# com.gamecore.resources

Type-agnostic resource economy. Games define their own resource types; this package handles the math and inventory management.

## What it does

`ResourceBundle` is an **immutable value object** — every `Add` / `Remove` returns a new instance. This makes it safe to pass bundles around as costs, offers, or snapshots without defensive copying.

`ResourceInventory` is **mutable** and owned by a player. It wraps a `ResourceBundle` and publishes events on every change so the UI and other systems react without polling.

## Key types

| Type | Role |
|---|---|
| `IResource` | Identity contract for a resource type (`ResourceId`, `DisplayName`) |
| `ResourceBundle` | Immutable amount map; `+` operator, `CanAfford`, `Add`, `Remove` |
| `IResourceInventory` | Interface for a mutable inventory |
| `ResourceInventory` | Concrete implementation; publishes add/remove/insufficient events |

## Events published

`ResourceAddedEvent`, `ResourceRemovedEvent`, `ResourceInsufficientEvent`, `ResourceTransferredEvent`

## Dependencies

`com.gamecore.events`, `com.gamecore.player`

## Implementation status

`ResourceBundle` and `ResourceInventory` are fully implemented. `ResourceInventory.TryRemove` publishes `ResourceInsufficientEvent` and returns false when the player can't afford the cost — callers must check the return value.
