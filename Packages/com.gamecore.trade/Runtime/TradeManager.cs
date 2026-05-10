using UnityEngine;
using GameCore.Player;
using GameCore.Resources;

namespace GameCore.Trade
{
    public class TradeManager : MonoBehaviour
    {
        public ITradeRule Rule { get; set; }

        public ITradeOffer ProposeTradeToPlayer(IPlayer proposer, IPlayer target, ResourceBundle offering, ResourceBundle requesting)
            => throw new System.NotImplementedException();

        public ITradeOffer ProposeTradeToBank(IPlayer proposer, ResourceBundle offering, ResourceBundle requesting)
            => throw new System.NotImplementedException();

        public bool AcceptTrade(ITradeOffer offer, IPlayer responder) => throw new System.NotImplementedException();
        public bool RejectTrade(ITradeOffer offer, IPlayer responder) => throw new System.NotImplementedException();
        public void CancelTrade(ITradeOffer offer) => throw new System.NotImplementedException();
    }
}
