using GameCore.Player;

namespace GameCore.Trade
{
    public interface ITradeRule
    {
        bool CanTrade(ITradeOffer offer, IPlayer responder);
        string GetRejectionReason(ITradeOffer offer, IPlayer responder);
    }
}
