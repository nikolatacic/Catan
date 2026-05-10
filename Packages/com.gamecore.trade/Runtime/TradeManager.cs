using System.Collections.Generic;
using UnityEngine;
using GameCore.Events;
using GameCore.Player;
using GameCore.Resources;

namespace GameCore.Trade
{
    public class TradeManager : MonoBehaviour
    {
        public ITradeRule Rule { get; set; }

        private readonly Dictionary<string, TradeOffer> _activeOffers = new();

        public ITradeOffer ProposeTradeToPlayer(
            IPlayer proposer, IPlayer target,
            ResourceBundle offering, ResourceBundle requesting)
        {
            var offer = new TradeOffer(proposer, target, offering, requesting);
            _activeOffers[offer.OfferId] = offer;
            EventBus.Publish(new TradeProposedEvent { Offer = offer });
            return offer;
        }

        public ITradeOffer ProposeTradeToBank(
            IPlayer proposer,
            ResourceBundle offering, ResourceBundle requesting)
        {
            var offer = new TradeOffer(proposer, null, offering, requesting);
            _activeOffers[offer.OfferId] = offer;
            EventBus.Publish(new TradeProposedEvent { Offer = offer });
            return offer;
        }

        // Validates via the injected rule and publishes TradeAcceptedEvent then TradeCompletedEvent.
        // Actual resource transfer is the caller's or a subscriber's responsibility.
        public bool AcceptTrade(ITradeOffer offer, IPlayer responder)
        {
            if (!_activeOffers.TryGetValue(offer.OfferId, out var activeOffer)) return false;
            if (activeOffer.Status != TradeStatus.Pending) return false;

            if (Rule != null && !Rule.CanTrade(offer, responder))
            {
                var reason = Rule.GetRejectionReason(offer, responder);
                activeOffer.Status = TradeStatus.Rejected;
                _activeOffers.Remove(offer.OfferId);
                EventBus.Publish(new TradeRejectedEvent { Offer = offer, Responder = responder, Reason = reason });
                return false;
            }

            activeOffer.Status = TradeStatus.Accepted;
            _activeOffers.Remove(offer.OfferId);

            EventBus.Publish(new TradeAcceptedEvent { Offer = offer, Responder = responder });
            EventBus.Publish(new TradeCompletedEvent { Offer = offer });
            return true;
        }

        public bool RejectTrade(ITradeOffer offer, IPlayer responder)
        {
            if (!_activeOffers.TryGetValue(offer.OfferId, out var activeOffer)) return false;
            if (activeOffer.Status != TradeStatus.Pending) return false;

            var reason = Rule?.GetRejectionReason(offer, responder) ?? "Trade rejected.";
            activeOffer.Status = TradeStatus.Rejected;
            _activeOffers.Remove(offer.OfferId);
            EventBus.Publish(new TradeRejectedEvent { Offer = offer, Responder = responder, Reason = reason });
            return true;
        }

        public void CancelTrade(ITradeOffer offer)
        {
            if (!_activeOffers.TryGetValue(offer.OfferId, out var activeOffer)) return;
            if (activeOffer.Status != TradeStatus.Pending) return;

            activeOffer.Status = TradeStatus.Cancelled;
            _activeOffers.Remove(offer.OfferId);
            EventBus.Publish(new TradeCancelledEvent { Offer = offer });
        }
    }
}
