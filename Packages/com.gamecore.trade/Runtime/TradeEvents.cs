using GameCore.Events;
using GameCore.Player;

namespace GameCore.Trade
{
    public struct TradeProposedEvent : IGameEvent { public ITradeOffer Offer; }
    public struct TradeAcceptedEvent : IGameEvent { public ITradeOffer Offer; public IPlayer Responder; }
    public struct TradeRejectedEvent : IGameEvent { public ITradeOffer Offer; public IPlayer Responder; public string Reason; }
    public struct TradeCompletedEvent : IGameEvent { public ITradeOffer Offer; }
    public struct TradeCancelledEvent : IGameEvent { public ITradeOffer Offer; }
}
