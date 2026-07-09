using Unity.Netcode;
using UnityEngine;
using GameCore.Board;
using GameCore.Player;
using GameCore.Resources;
using Catan.UI;

namespace Catan.Network
{
    // ── In-scene NetworkObject — lives in MainScene ───────────────────────────
    // Sits on a GameObject with NetworkObject component. Host's GameManager is
    // the authority; clients send commands via the ServerRpc methods below.
    //
    // Vertex/Edge identification: encoded as (HexCoord tile, byte index 0-5).
    // The HexGrid topology is deterministic from board size, so this resolves
    // to the same vertex/edge on both peers. Tile *contents* (resources, dice
    // numbers) can still differ until state sync lands in Phase 5c.
    //
    // Player/Resource identification: int index + CatanResourceType enum.
    // ──────────────────────────────────────────────────────────────────────────

    public class NetworkCommandBridge : NetworkBehaviour
    {
        public static NetworkCommandBridge Instance { get; private set; }

        public override void OnNetworkSpawn()
        {
            Instance = this;
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;
        }

        // ── Turn flow ──────────────────────────────────────────────────────────

        [ServerRpc(RequireOwnership = false)]
        public void RequestRollServerRpc(ServerRpcParams rpc = default)
            => GameManager.Instance?.RequestRoll();

        [ServerRpc(RequireOwnership = false)]
        public void EndTurnServerRpc(ServerRpcParams rpc = default)
            => GameManager.Instance?.EndTurn();

        // ── Building ───────────────────────────────────────────────────────────

        [ServerRpc(RequireOwnership = false)]
        public void PlaceSettlementServerRpc(int tileQ, int tileR, byte cornerIndex,
            ServerRpcParams rpc = default)
        {
            var vertex = LookupVertex(tileQ, tileR, cornerIndex);
            if (vertex != null) GameManager.Instance?.TryPlaceSettlement(vertex);
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlaceRoadServerRpc(int tileQ, int tileR, byte edgeIndex,
            ServerRpcParams rpc = default)
        {
            var edge = LookupEdge(tileQ, tileR, edgeIndex);
            if (edge != null) GameManager.Instance?.TryPlaceRoad(edge);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpgradeCityServerRpc(int tileQ, int tileR, byte cornerIndex,
            ServerRpcParams rpc = default)
        {
            var vertex = LookupVertex(tileQ, tileR, cornerIndex);
            if (vertex != null) GameManager.Instance?.TryUpgradeCity(vertex);
        }

        // ── Robber ─────────────────────────────────────────────────────────────

        [ServerRpc(RequireOwnership = false)]
        public void MoveRobberServerRpc(int q, int r, ServerRpcParams rpc = default)
            => GameManager.Instance?.TryMoveRobber(new HexCoord(q, r));

        [ServerRpc(RequireOwnership = false)]
        public void CompleteRobberMoveServerRpc(int q, int r, int victimIndex,
            ServerRpcParams rpc = default)
        {
            var manager = GameManager.Instance;
            if (manager == null) return;

            IPlayer victim = null;
            if (victimIndex >= 0 && victimIndex < manager.Players.Count)
                victim = manager.Players[victimIndex];

            manager.CompleteRobberMove(new HexCoord(q, r), victim);
        }

        // ── Dev cards ──────────────────────────────────────────────────────────

        [ServerRpc(RequireOwnership = false)]
        public void PurchaseDevCardServerRpc(ServerRpcParams rpc = default)
            => GameManager.Instance?.TryPurchaseDevCard();

        // Identifies a dev card by its index in the *active* player's hand at
        // the time of dispatch. Acceptable because dev cards are played by the
        // current active player only, and the host validates against its own
        // copy of that hand.
        [ServerRpc(RequireOwnership = false)]
        public void PlayDevCardServerRpc(int cardIndex, ServerRpcParams rpc = default)
        {
            var manager = GameManager.Instance;
            var player  = manager?.ActivePlayer;
            if (player == null) return;

            var cards = player.DevelopmentCards.Cards;
            if (cardIndex < 0 || cardIndex >= cards.Count) return;

            manager.TryPlayDevCard(cards[cardIndex]);
        }

        // ── Trading ────────────────────────────────────────────────────────────

        [ServerRpc(RequireOwnership = false)]
        public void BankTradeServerRpc(byte giveType, byte receiveType,
            ServerRpcParams rpc = default)
        {
            var give    = CatanResources.Get((CatanResourceType)giveType);
            var receive = CatanResources.Get((CatanResourceType)receiveType);
            GameManager.Instance?.TryBankTrade(give, receive);
        }

        // ── Player trading ─────────────────────────────────────────────────────

        [ServerRpc(RequireOwnership = false)]
        public void ProposePlayerTradeServerRpc(
            byte targetPlayerIndex,
            byte offerWood, byte offerBrick, byte offerSheep, byte offerWheat, byte offerOre,
            byte wantWood,  byte wantBrick,  byte wantSheep,  byte wantWheat,  byte wantOre,
            ServerRpcParams rpc = default)
        {
            var manager = GameManager.Instance;
            if (manager == null) return;
            var offering   = BundleFromBytes(offerWood, offerBrick, offerSheep, offerWheat, offerOre);
            var requesting = BundleFromBytes(wantWood,  wantBrick,  wantSheep,  wantWheat,  wantOre);
            manager.TryProposePlayerTrade(targetPlayerIndex, offering, requesting);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AcceptPlayerTradeServerRpc(byte acceptingPlayerIndex, ServerRpcParams rpc = default)
            => GameManager.Instance?.TryAcceptPlayerTrade(acceptingPlayerIndex);

        [ServerRpc(RequireOwnership = false)]
        public void DeclinePlayerTradeServerRpc(byte decliningPlayerIndex, ServerRpcParams rpc = default)
            => GameManager.Instance?.TryDeclinePlayerTrade(decliningPlayerIndex);

        [ServerRpc(RequireOwnership = false)]
        public void CounterPlayerTradeServerRpc(
            byte counteringPlayerIndex,
            byte counterOfferWood, byte counterOfferBrick, byte counterOfferSheep, byte counterOfferWheat, byte counterOfferOre,
            byte counterWantWood,  byte counterWantBrick,  byte counterWantSheep,  byte counterWantWheat,  byte counterWantOre,
            ServerRpcParams rpc = default)
        {
            var manager = GameManager.Instance;
            if (manager == null) return;
            var counterOffering   = BundleFromBytes(counterOfferWood, counterOfferBrick, counterOfferSheep, counterOfferWheat, counterOfferOre);
            var counterRequesting = BundleFromBytes(counterWantWood,  counterWantBrick,  counterWantSheep,  counterWantWheat,  counterWantOre);
            manager.TryCounterPlayerTrade(counteringPlayerIndex, counterOffering, counterRequesting);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CancelPlayerTradeServerRpc(ServerRpcParams rpc = default)
            => GameManager.Instance?.TryCancelPlayerTrade();

        // ── Topology lookups ───────────────────────────────────────────────────

        private static ResourceBundle BundleFromBytes(byte wood, byte brick, byte sheep, byte wheat, byte ore)
        {
            var bundle = new ResourceBundle();
            if (wood  > 0) bundle = bundle.Add(CatanResources.Wood,  wood);
            if (brick > 0) bundle = bundle.Add(CatanResources.Brick, brick);
            if (sheep > 0) bundle = bundle.Add(CatanResources.Sheep, sheep);
            if (wheat > 0) bundle = bundle.Add(CatanResources.Wheat, wheat);
            if (ore   > 0) bundle = bundle.Add(CatanResources.Ore,   ore);
            return bundle;
        }

        private static HexVertex LookupVertex(int q, int r, byte cornerIndex)
        {
            var board = GameManager.Instance?.Board;
            if (board == null) return null;
            var vertices = board.Grid.GetVertices(new HexCoord(q, r));
            return cornerIndex < vertices.Count ? vertices[cornerIndex] : null;
        }

        private static HexEdge LookupEdge(int q, int r, byte edgeIndex)
        {
            var board = GameManager.Instance?.Board;
            if (board == null) return null;
            var edges = board.Grid.GetEdges(new HexCoord(q, r));
            return edgeIndex < edges.Count ? edges[edgeIndex] : null;
        }
    }
}
