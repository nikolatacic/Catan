using GameCore.Player;
using GameCore.Resources;

namespace GameCore.Trade
{
    public interface ITradeOffer
    {
        string OfferId { get; }
        IPlayer Proposer { get; }
        IPlayer Target { get; }
        ResourceBundle Offering { get; }
        ResourceBundle Requesting { get; }
        TradeStatus Status { get; }
    }
}
