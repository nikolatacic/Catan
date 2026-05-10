using System.Collections.Generic;
using System.Linq;
using GameCore.Board;
using GameCore.Events;
using GameCore.Player;

namespace Catan
{
    public struct LongestRoadChangedEvent : IGameEvent { public IPlayer Previous; public IPlayer Current; }

    public class LongestRoadTracker
    {
        private const int MinimumRoadsToQualify = 5;

        public IPlayer CurrentHolder { get; private set; }
        public int CurrentLength { get; private set; }

        // Recalculates longest road for every player and transfers the token when needed.
        // Call this after any road is placed or a settlement changes ownership.
        public void Recalculate(CatanBoard board)
        {
            var allPlayers = board.Roads.Values
                .Select(road => road.Owner)
                .Distinct()
                .ToList();

            IPlayer newHolder = CurrentHolder;
            int newLength = CurrentLength;

            foreach (var player in allPlayers)
            {
                int playerLength = ComputeLongestRoad(player, board);

                if (playerLength >= MinimumRoadsToQualify && playerLength > newLength)
                {
                    newHolder = player;
                    newLength = playerLength;
                }
            }

            if (newHolder == CurrentHolder) return;

            var previousHolder = CurrentHolder;

            if (previousHolder is CatanPlayer previousCatanPlayer)
                previousCatanPlayer.HasLongestRoad = false;

            CurrentHolder = newHolder;
            CurrentLength = newLength;

            if (newHolder is CatanPlayer newCatanPlayer)
                newCatanPlayer.HasLongestRoad = true;

            EventBus.Publish(new LongestRoadChangedEvent { Previous = previousHolder, Current = newHolder });
        }

        // ── DFS road length algorithm ──────────────────────────────────────────

        private static int ComputeLongestRoad(IPlayer player, CatanBoard board)
        {
            var playerRoadEdges = board.Roads
                .Where(pair => pair.Value.Owner == player)
                .Select(pair => pair.Key)
                .ToList();

            if (playerRoadEdges.Count == 0) return 0;

            // Vertices where an opponent has a settlement — these block road continuity.
            var opponentSettledVertices = new HashSet<HexVertex>(
                board.Settlements
                    .Where(pair => pair.Value.Owner != player)
                    .Select(pair => pair.Key));

            // Build adjacency: vertex → list of (edge, neighbouring vertex).
            var adjacency = new Dictionary<HexVertex, List<(HexEdge edge, HexVertex neighbour)>>();
            foreach (var edge in playerRoadEdges)
            {
                if (edge.AdjacentVertices == null || edge.AdjacentVertices.Length < 2) continue;
                var vertexA = edge.AdjacentVertices[0];
                var vertexB = edge.AdjacentVertices[1];

                if (!adjacency.ContainsKey(vertexA)) adjacency[vertexA] = new();
                if (!adjacency.ContainsKey(vertexB)) adjacency[vertexB] = new();

                adjacency[vertexA].Add((edge, vertexB));
                adjacency[vertexB].Add((edge, vertexA));
            }

            int maxLength = 0;
            var visitedEdges = new HashSet<HexEdge>();

            foreach (var startVertex in adjacency.Keys)
            {
                int length = Dfs(startVertex, visitedEdges, adjacency, opponentSettledVertices);
                if (length > maxLength) maxLength = length;
            }

            return maxLength;
        }

        // Returns the longest road reachable from `current` without revisiting any edge.
        // An opponent's settlement on `current` prevents continuing through it.
        private static int Dfs(
            HexVertex current,
            HashSet<HexEdge> visitedEdges,
            Dictionary<HexVertex, List<(HexEdge edge, HexVertex neighbour)>> adjacency,
            HashSet<HexVertex> opponentSettledVertices)
        {
            if (opponentSettledVertices.Contains(current)) return 0;
            if (!adjacency.TryGetValue(current, out var neighbours)) return 0;

            int maxLength = 0;
            foreach (var (edge, neighbour) in neighbours)
            {
                if (visitedEdges.Contains(edge)) continue;

                visitedEdges.Add(edge);
                int length = 1 + Dfs(neighbour, visitedEdges, adjacency, opponentSettledVertices);
                if (length > maxLength) maxLength = length;
                visitedEdges.Remove(edge);
            }
            return maxLength;
        }
    }
}
