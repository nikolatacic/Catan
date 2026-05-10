# com.gamecore.player

Minimal player identity contract. Games extend `IPlayer` with game-specific state rather than adding fields here.

## What it does

Defines what a "player" is at the infrastructure level: an ID, a display name, and a color. The concrete `PlayerData` ScriptableObject lets designers configure players in the inspector. `PlayerManager` holds the active roster and can be queried by any system that needs a player reference.

## Key types

| Type | Role |
|---|---|
| `IPlayer` | Identity contract: `Id`, `DisplayName`, `Color` |
| `PlayerData` | `[CreateAssetMenu]` ScriptableObject implementing `IPlayer` |
| `PlayerManager` | MonoBehaviour roster; add/remove/query players at runtime |

## Events published

`PlayerJoinedEvent`, `PlayerLeftEvent`, `ActivePlayerChangedEvent`

## Dependencies

`com.gamecore.events`

## Extension point

Catan extends this with `CatanPlayer` (in `Assets/Catan/Players/`) which adds resources, cards, pieces, and knight/road tracking. No fields are added to `IPlayer` itself.
