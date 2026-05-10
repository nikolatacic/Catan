# com.gamecore.score

Pluggable victory condition evaluation. Multiple `IVictoryCondition` instances contribute points independently; the manager sums them and checks for a winner.

## What it does

`ScoreManager.RecalculateAll` iterates every registered `IVictoryCondition` for every player, sums the results, and publishes `ScoreChangedEvent` if any score changed. It then calls `CheckVictory()` — if any player's condition returns `IsWinCondition = true`, `VictoryAchievedEvent` is published and the game ends.

## Key types

| Type | Role |
|---|---|
| `IVictoryCondition` | Single scoring axis: `CalculatePoints` + `IsWinCondition` |
| `ScoreManager` | MonoBehaviour; holds condition list, triggers recalculation |

## Events published

`ScoreChangedEvent`, `LeaderChangedEvent`, `VictoryAchievedEvent`

## Dependencies

`com.gamecore.events`, `com.gamecore.player`

## Extension point

`CatanVictoryCondition` (in `Assets/Catan/Score/`) counts settlements, cities, VP cards, largest army, and longest road. Additional conditions (e.g. a house rule variant) can be added to `ScoreManager.Conditions` without changing any existing code.
