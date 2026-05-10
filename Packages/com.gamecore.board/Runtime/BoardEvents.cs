using GameCore.Events;

namespace GameCore.Board
{
    public struct TileSelectedEvent : IGameEvent { public HexCoord Coord; }
    public struct VertexSelectedEvent : IGameEvent { public HexVertex Vertex; }
    public struct EdgeSelectedEvent : IGameEvent { public HexEdge Edge; }
}
