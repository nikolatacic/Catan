using System;
using GameCore.Player;
using GameCore.Resources;

namespace GameCore.Trade
{
    public class TradeOffer : ITradeOffer
    {
        public string OfferId { get; } = Guid.NewGuid().ToString();
        public IPlayer Proposer { get; }
        public IPlayer Target { get; }
        public ResourceBundle Offering { get; }
        public ResourceBundle Requesting { get; }
        public TradeStatus Status { get; internal set; } = TradeStatus.Pending;

        public TradeOffer(IPlayer proposer, IPlayer target, ResourceBundle offering, ResourceBundle requesting)
        {
            Proposer = proposer;
            Target = target;
            Offering = offering;
            Requesting = requesting;
        }
    }
}
