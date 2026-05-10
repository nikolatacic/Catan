using GameCore.Player;
using GameCore.Trade;

namespace Catan
{
    public class CatanTradeRule : ITradeRule
    {
        private CatanTurnManager _turn;
        private PortSystem _ports;

        public CatanTradeRule(CatanTurnManager turn, PortSystem ports)
        {
            _turn = turn;
            _ports = ports;
        }

        public bool CanTrade(ITradeOffer offer, IPlayer responder) => throw new System.NotImplementedException();
        public string GetRejectionReason(ITradeOffer offer, IPlayer responder) => throw new System.NotImplementedException();
    }
}
