using GameCore.Board;
using GameCore.Player;
using GameCore.Resources;

namespace Catan.Commands
{
    // ── Turn flow ──────────────────────────────────────────────────────────────

    public class RequestRollCommand : IGameCommand
    {
        public void Execute(UI.GameManager manager) => manager.RequestRoll();
    }

    public class EndTurnCommand : IGameCommand
    {
        public void Execute(UI.GameManager manager) => manager.EndTurn();
    }

    // ── Building ───────────────────────────────────────────────────────────────

    public class PlaceSettlementCommand : IGameCommand
    {
        public HexVertex Vertex;
        public void Execute(UI.GameManager manager) => manager.TryPlaceSettlement(Vertex);
    }

    public class PlaceRoadCommand : IGameCommand
    {
        public HexEdge Edge;
        public void Execute(UI.GameManager manager) => manager.TryPlaceRoad(Edge);
    }

    public class UpgradeCityCommand : IGameCommand
    {
        public HexVertex Vertex;
        public void Execute(UI.GameManager manager) => manager.TryUpgradeCity(Vertex);
    }

    // ── Robber ─────────────────────────────────────────────────────────────────

    public class MoveRobberCommand : IGameCommand
    {
        public HexCoord Coord;
        public void Execute(UI.GameManager manager) => manager.TryMoveRobber(Coord);
    }

    // Used when MoveRobberCommand triggers a multi-victim choice; the
    // StealTargetPanelView dispatches this once the victim is picked.
    public class CompleteRobberMoveCommand : IGameCommand
    {
        public HexCoord Coord;
        public IPlayer Victim;
        public void Execute(UI.GameManager manager) => manager.CompleteRobberMove(Coord, Victim);
    }

    // ── Dev cards ──────────────────────────────────────────────────────────────

    public class PurchaseDevCardCommand : IGameCommand
    {
        public void Execute(UI.GameManager manager) => manager.TryPurchaseDevCard();
    }

    public class PlayDevCardCommand : IGameCommand
    {
        public DevelopmentCard Card;
        public void Execute(UI.GameManager manager) => manager.TryPlayDevCard(Card);
    }

    // ── Trading ────────────────────────────────────────────────────────────────

    public class BankTradeCommand : IGameCommand
    {
        public IResource Give;
        public IResource Receive;
        public void Execute(UI.GameManager manager) => manager.TryBankTrade(Give, Receive);
    }
}
