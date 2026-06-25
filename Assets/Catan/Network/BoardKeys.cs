using GameCore.Board;

namespace Catan.Network
{
    // ── Stable identifiers for board topology objects ─────────────────────────
    // Vertices/edges are object instances in the host's Board. To send a
    // command across the wire, encode them as (HexCoord tile, byte index 0-5)
    // where index is the position in HexGrid.GetVertices/GetEdges for that tile.
    //
    // Topology is deterministic (depends only on board size, not seed), so the
    // same (tile, index) resolves to the equivalent vertex/edge on every peer.
    // ──────────────────────────────────────────────────────────────────────────

    public static class BoardKeys
    {
        public static bool TryGetVertexKey(HexVertex vertex, UI.GameManager manager,
            out HexCoord tile, out byte cornerIndex)
        {
            tile = default;
            cornerIndex = 0;
            if (vertex == null || vertex.AdjacentTiles == null || vertex.AdjacentTiles.Length == 0)
                return false;
            if (manager?.Board == null) return false;

            // Walk every adjacent tile until we find a tile where this vertex
            // appears in GetVertices. Both peers do the same walk, so the chosen
            // tile resolves identically on the host.
            foreach (var candidateTile in vertex.AdjacentTiles)
            {
                var tileVertices = manager.Board.Grid.GetVertices(candidateTile);
                for (byte index = 0; index < tileVertices.Count; index++)
                {
                    if (tileVertices[index] == vertex)
                    {
                        tile = candidateTile;
                        cornerIndex = index;
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool TryGetEdgeKey(HexEdge edge, UI.GameManager manager,
            out HexCoord tile, out byte edgeIndex)
        {
            tile = default;
            edgeIndex = 0;
            if (edge == null || edge.AdjacentTiles == null || edge.AdjacentTiles.Length == 0)
                return false;
            if (manager?.Board == null) return false;

            foreach (var candidateTile in edge.AdjacentTiles)
            {
                var tileEdges = manager.Board.Grid.GetEdges(candidateTile);
                for (byte index = 0; index < tileEdges.Count; index++)
                {
                    if (tileEdges[index] == edge)
                    {
                        tile = candidateTile;
                        edgeIndex = index;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
