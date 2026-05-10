# com.gamecore.board

Pure data package — no rendering, no MonoBehaviours. Provides hex math, grid storage, and the topological graph of tiles, vertices, and edges.

## What it does

- Axial coordinate system (`HexCoord`) with neighbor lookup, distance, and world-position conversion
- `HexGrid<T>` keyed by `HexCoord` for storing any tile type
- `HexVertex` and `HexEdge` represent the intersections and edges shared between tiles — used by the build system to place settlements and roads

## Key types

| Type | Role |
|---|---|
| `HexCoord` | Axial (Q, R) coordinate; all hex math lives here |
| `IHexTile` | Marker interface — any tile type must implement this |
| `HexGrid<T>` | Dictionary-backed grid; provides neighbour and topology queries |
| `HexVertex` | Shared corner of 2–3 tiles; settlement placement sites |
| `HexEdge` | Shared side between 2 tiles; road placement sites |
| `BoardGenerator<T>` | Abstract base; subclass to produce a specific board layout |

## Events published

`TileSelectedEvent`, `VertexSelectedEvent`, `EdgeSelectedEvent` — fired by UI input handlers, consumed by the build system and game rules.

## Dependencies

`com.gamecore.events`

## Note

`HexCoord` is fully implemented (math, round, world conversion). `HexGrid.GetVertices` and `GetEdges` are stubbed — they will be completed in Phase 3 when the full vertex/edge topology is built during board generation.
