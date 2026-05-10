# com.gamecore.build

Placement and removal of game pieces on board locations. All legality logic lives in an injected `IBuildRule`.

## What it does

`BuildManager.TryPlace` runs the rule check, deducts resources from the player, places the piece, and publishes `BuildSucceededEvent` — or publishes `BuildFailedEvent` with a reason string if the rule rejects it. Callers never need to check rules themselves.

## Key types

| Type | Role |
|---|---|
| `IPlaceable` | A piece that can be placed: `PlaceableId` + `BuildCost` |
| `IBuildLocation` | A slot on the board: `IsOccupied` + `OccupiedBy` |
| `IBuildRule` | Validates placement and removal; implemented by Catan |
| `BuildManager` | MonoBehaviour; inject `Rule` before first build action |

## Events published

`BuildAttemptedEvent`, `BuildSucceededEvent`, `BuildFailedEvent`, `PieceRemovedEvent`

## Dependencies

`com.gamecore.events`, `com.gamecore.player`, `com.gamecore.resources`, `com.gamecore.board`

## Extension point

`CatanBuildRule` (in `Assets/Catan/Rules/`) implements the distance rule, road-connection check, and city-upgrade logic. `BuildSucceededEvent` is consumed by `LongestRoadTracker` and `ScoreManager` after each successful placement.
