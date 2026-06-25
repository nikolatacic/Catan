using GameCore.Board;
using GameCore.Build;
using GameCore.Player;
using GameCore.Resources;

namespace Catan
{
    public class CatanBuildRule : IBuildRule
    {
        private readonly CatanBoard _board;
        private readonly CatanTurnManager _turn;

        public CatanBuildRule(CatanBoard board, CatanTurnManager turn = null)
        {
            _board = board;
            _turn = turn;
        }

        public bool CanPlace(IPlaceable piece, IBuildLocation location, IPlayer player)
            => GetPlacementFailureReason(piece, player) == null;

        public bool CanRemove(IPlaceable piece, IBuildLocation location, IPlayer player)
            => false;

        public string GetFailureReason(IPlaceable piece, IBuildLocation location, IPlayer player)
            => GetPlacementFailureReason(piece, player) ?? "Cannot remove pieces in Catan.";

        // ── Private validation ─────────────────────────────────────────────────

        private string GetPlacementFailureReason(IPlaceable piece, IPlayer player)
        {
            if (player is not CatanPlayer catanPlayer)
                return "Player is not a CatanPlayer.";

            bool isSetupPhase = _turn?.CurrentCatanPhase == CatanTurnPhase.SetupPlacement;

            return piece switch
            {
                Settlement settlement when !settlement.IsCity =>
                    CheckSettlementPlacement(settlement.Location, catanPlayer, isSetupPhase),
                Settlement city when city.IsCity =>
                    CheckCityUpgrade(city.Location, catanPlayer),
                Road road =>
                    CheckRoadPlacement(road.Location, catanPlayer, isSetupPhase),
                _ => "Unknown piece type."
            };
        }

        private string CheckSettlementPlacement(HexVertex vertex, CatanPlayer player, bool isSetupPhase)
        {
            if (vertex == null)
                return "Invalid vertex.";

            if (_board.Settlements.ContainsKey(vertex))
                return "Vertex is already occupied.";

            if (!IsDistanceRuleSatisfied(vertex))
                return "Distance rule violated: a settlement is already adjacent.";

            if (isSetupPhase)
                return null;

            if (!HasRoadConnectionAtVertex(vertex, player))
                return "No connected road at this vertex.";

            if (!player.Resources.CanAfford(SettlementCost()))
                return "Insufficient resources to build a settlement.";

            return null;
        }

        private string CheckCityUpgrade(HexVertex vertex, CatanPlayer player)
        {
            if (vertex == null)
                return "Invalid vertex.";

            if (!_board.Settlements.TryGetValue(vertex, out var existingSettlement))
                return "No settlement at this vertex to upgrade.";

            if (existingSettlement.Owner != player)
                return "Cannot upgrade another player's settlement.";

            // IsCity is already true here because GameManager calls UpgradeToCity() before
            // TryPlace to route through the switch. Double-upgrade is guarded in GameManager.
            if (!player.Resources.CanAfford(CityCost()))
                return "Insufficient resources to build a city.";

            return null;
        }

        private string CheckRoadPlacement(HexEdge edge, CatanPlayer player, bool isSetupPhase)
        {
            if (edge == null)
                return "Invalid edge.";

            if (_board.Roads.ContainsKey(edge))
                return "An edge is already occupied.";

            if (!HasRoadConnectionAtEdge(edge, player))
                return "No connected road or settlement at this edge.";

            if (isSetupPhase)
                return null;

            if (!player.Resources.CanAfford(RoadCost()))
                return "Insufficient resources to build a road.";

            return null;
        }

        // Returns true when no vertex adjacent (through an edge) to the target already has a settlement.
        private bool IsDistanceRuleSatisfied(HexVertex vertex)
        {
            if (vertex.AdjacentEdges == null) return true;

            foreach (var adjacentEdge in vertex.AdjacentEdges)
            {
                if (adjacentEdge.AdjacentVertices == null) continue;
                foreach (var neighbourVertex in adjacentEdge.AdjacentVertices)
                {
                    if (neighbourVertex == vertex) continue;
                    if (_board.Settlements.ContainsKey(neighbourVertex)) return false;
                }
            }
            return true;
        }

        // Returns true when the player has an own road touching the given vertex,
        // or an own settlement already there (which never happens for initial placement but guards upgrades).
        private bool HasRoadConnectionAtVertex(HexVertex vertex, CatanPlayer player)
        {
            if (vertex.AdjacentEdges == null) return false;

            foreach (var adjacentEdge in vertex.AdjacentEdges)
            {
                if (_board.Roads.TryGetValue(adjacentEdge, out var road) && road.Owner == player)
                    return true;
            }
            return false;
        }

        // Returns true when at least one endpoint of the edge either:
        //   a) has the player's own settlement, or
        //   b) has the player's own road on a neighbouring edge AND no opponent settlement blocks the endpoint.
        private bool HasRoadConnectionAtEdge(HexEdge edge, CatanPlayer player)
        {
            if (edge.AdjacentVertices == null) return false;

            foreach (var vertex in edge.AdjacentVertices)
            {
                // Own settlement at endpoint — always a valid connection.
                if (_board.Settlements.TryGetValue(vertex, out var settlement))
                {
                    if (settlement.Owner == player) return true;
                    // Opponent's settlement — cannot extend a road through this vertex.
                    continue;
                }

                // Empty vertex — check for adjacent roads owned by the player.
                if (vertex.AdjacentEdges == null) continue;
                foreach (var neighbourEdge in vertex.AdjacentEdges)
                {
                    if (neighbourEdge == edge) continue;
                    if (_board.Roads.TryGetValue(neighbourEdge, out var neighbourRoad)
                        && neighbourRoad.Owner == player)
                        return true;
                }
            }
            return false;
        }

        // ── Build costs ────────────────────────────────────────────────────────

        private static ResourceBundle SettlementCost() =>
            new ResourceBundle()
                .Add(CatanResources.Wood, 1)
                .Add(CatanResources.Brick, 1)
                .Add(CatanResources.Sheep, 1)
                .Add(CatanResources.Wheat, 1);

        private static ResourceBundle CityCost() =>
            new ResourceBundle()
                .Add(CatanResources.Wheat, 2)
                .Add(CatanResources.Ore, 3);

        private static ResourceBundle RoadCost() =>
            new ResourceBundle()
                .Add(CatanResources.Wood, 1)
                .Add(CatanResources.Brick, 1);
    }
}
