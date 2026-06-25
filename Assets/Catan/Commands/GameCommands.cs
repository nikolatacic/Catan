using UnityEngine;
using GameCore.Board;
using GameCore.Player;
using GameCore.Resources;
using Catan.Network;

namespace Catan.Commands
{
    // ── Turn flow ──────────────────────────────────────────────────────────────

    public class RequestRollCommand : IGameCommand
    {
        public void Execute(UI.GameManager manager) => manager.RequestRoll();
        public void SendOverNetwork(NetworkCommandBridge bridge) => bridge.RequestRollServerRpc();
    }

    public class EndTurnCommand : IGameCommand
    {
        public void Execute(UI.GameManager manager) => manager.EndTurn();
        public void SendOverNetwork(NetworkCommandBridge bridge) => bridge.EndTurnServerRpc();
    }

    // ── Building ───────────────────────────────────────────────────────────────

    public class PlaceSettlementCommand : IGameCommand
    {
        public HexVertex Vertex;
        public void Execute(UI.GameManager manager) => manager.TryPlaceSettlement(Vertex);
        public void SendOverNetwork(NetworkCommandBridge bridge)
        {
            if (!BoardKeys.TryGetVertexKey(Vertex, UI.GameManager.Instance, out var tile, out var corner))
            {
                Debug.LogWarning("[PlaceSettlementCommand] Could not resolve vertex to a key.");
                return;
            }
            bridge.PlaceSettlementServerRpc(tile.Q, tile.R, corner);
        }
    }

    public class PlaceRoadCommand : IGameCommand
    {
        public HexEdge Edge;
        public void Execute(UI.GameManager manager) => manager.TryPlaceRoad(Edge);
        public void SendOverNetwork(NetworkCommandBridge bridge)
        {
            if (!BoardKeys.TryGetEdgeKey(Edge, UI.GameManager.Instance, out var tile, out var index))
            {
                Debug.LogWarning("[PlaceRoadCommand] Could not resolve edge to a key.");
                return;
            }
            bridge.PlaceRoadServerRpc(tile.Q, tile.R, index);
        }
    }

    public class UpgradeCityCommand : IGameCommand
    {
        public HexVertex Vertex;
        public void Execute(UI.GameManager manager) => manager.TryUpgradeCity(Vertex);
        public void SendOverNetwork(NetworkCommandBridge bridge)
        {
            if (!BoardKeys.TryGetVertexKey(Vertex, UI.GameManager.Instance, out var tile, out var corner))
            {
                Debug.LogWarning("[UpgradeCityCommand] Could not resolve vertex to a key.");
                return;
            }
            bridge.UpgradeCityServerRpc(tile.Q, tile.R, corner);
        }
    }

    // ── Robber ─────────────────────────────────────────────────────────────────

    public class MoveRobberCommand : IGameCommand
    {
        public HexCoord Coord;
        public void Execute(UI.GameManager manager) => manager.TryMoveRobber(Coord);
        public void SendOverNetwork(NetworkCommandBridge bridge)
            => bridge.MoveRobberServerRpc(Coord.Q, Coord.R);
    }

    public class CompleteRobberMoveCommand : IGameCommand
    {
        public HexCoord Coord;
        public IPlayer Victim;
        public void Execute(UI.GameManager manager) => manager.CompleteRobberMove(Coord, Victim);
        public void SendOverNetwork(NetworkCommandBridge bridge)
        {
            int victimIndex = -1;
            var manager = UI.GameManager.Instance;
            if (Victim != null && manager != null)
                victimIndex = manager.Players.IndexOf(Victim as CatanPlayer);
            bridge.CompleteRobberMoveServerRpc(Coord.Q, Coord.R, victimIndex);
        }
    }

    // ── Dev cards ──────────────────────────────────────────────────────────────

    public class PurchaseDevCardCommand : IGameCommand
    {
        public void Execute(UI.GameManager manager) => manager.TryPurchaseDevCard();
        public void SendOverNetwork(NetworkCommandBridge bridge)
            => bridge.PurchaseDevCardServerRpc();
    }

    public class PlayDevCardCommand : IGameCommand
    {
        public DevelopmentCard Card;
        public void Execute(UI.GameManager manager) => manager.TryPlayDevCard(Card);
        public void SendOverNetwork(NetworkCommandBridge bridge)
        {
            var manager = UI.GameManager.Instance;
            var player  = manager?.ActivePlayer;
            int index = -1;
            if (player != null && Card != null)
            {
                var cards = player.DevelopmentCards.Cards;
                for (int i = 0; i < cards.Count; i++)
                {
                    if (cards[i] == Card) { index = i; break; }
                }
            }
            if (index < 0)
            {
                Debug.LogWarning("[PlayDevCardCommand] Card not found in active player's hand.");
                return;
            }
            bridge.PlayDevCardServerRpc(index);
        }
    }

    // ── Trading ────────────────────────────────────────────────────────────────

    public class BankTradeCommand : IGameCommand
    {
        public IResource Give;
        public IResource Receive;
        public void Execute(UI.GameManager manager) => manager.TryBankTrade(Give, Receive);
        public void SendOverNetwork(NetworkCommandBridge bridge)
        {
            byte? giveType    = TypeOf(Give);
            byte? receiveType = TypeOf(Receive);
            if (giveType == null || receiveType == null)
            {
                Debug.LogWarning("[BankTradeCommand] Unrecognised resource.");
                return;
            }
            bridge.BankTradeServerRpc(giveType.Value, receiveType.Value);
        }

        private static byte? TypeOf(IResource resource)
        {
            if (resource == CatanResources.Wood)  return (byte)CatanResourceType.Wood;
            if (resource == CatanResources.Brick) return (byte)CatanResourceType.Brick;
            if (resource == CatanResources.Sheep) return (byte)CatanResourceType.Sheep;
            if (resource == CatanResources.Wheat) return (byte)CatanResourceType.Wheat;
            if (resource == CatanResources.Ore)   return (byte)CatanResourceType.Ore;
            return null;
        }
    }
}
