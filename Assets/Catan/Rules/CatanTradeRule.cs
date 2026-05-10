using GameCore.Player;
using GameCore.Resources;
using GameCore.Trade;

namespace Catan
{
    public class CatanTradeRule : ITradeRule
    {
        private readonly CatanTurnManager _turn;
        private readonly PortSystem _ports;

        public CatanTradeRule(CatanTurnManager turn, PortSystem ports)
        {
            _turn = turn;
            _ports = ports;
        }

        public bool CanTrade(ITradeOffer offer, IPlayer responder)
            => GetTradeFailureReason(offer, responder) == null;

        public string GetRejectionReason(ITradeOffer offer, IPlayer responder)
            => GetTradeFailureReason(offer, responder) ?? "Trade is valid.";

        // ── Private validation ─────────────────────────────────────────────────

        private string GetTradeFailureReason(ITradeOffer offer, IPlayer responder)
        {
            if (_turn.CurrentCatanPhase != CatanTurnPhase.Trading)
                return "Trading is only allowed during the trading phase.";

            if (offer.Target == null)
                return ValidateBankTrade(offer);

            return ValidatePlayerTrade(offer, responder);
        }

        // Bank/port trade: proposer offers a multiple of their trade ratio for one resource type,
        // and receives the negotiated amount of another resource type.
        private string ValidateBankTrade(ITradeOffer offer)
        {
            if (!CanAfford(offer.Proposer, offer.Offering))
                return "Proposer cannot afford the offered resources.";

            // Each resource in the offering must be offered in an exact multiple of the trade ratio.
            foreach (var pair in offer.Offering.Amounts)
            {
                if (pair.Value <= 0) continue;

                var resourceType = ResolveResourceType(pair.Key);
                if (!resourceType.HasValue)
                    return $"Resource '{pair.Key.ResourceId}' is not a valid Catan resource.";

                int tradeRatio = _ports.GetTradeRatio(offer.Proposer, resourceType.Value);
                if (pair.Value % tradeRatio != 0)
                    return $"Must offer an exact multiple of {tradeRatio} {pair.Key.DisplayName} for this trade ratio.";
            }

            return null;
        }

        // Player-to-player trade: both sides must be able to afford what they are giving.
        private string ValidatePlayerTrade(ITradeOffer offer, IPlayer responder)
        {
            if (!CanAfford(offer.Proposer, offer.Offering))
                return "Proposer cannot afford the offered resources.";

            if (responder == null)
                return "No responder specified for a player trade.";

            if (!CanAfford(responder, offer.Requesting))
                return "Responder cannot afford the requested resources.";

            return null;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static bool CanAfford(IPlayer player, ResourceBundle bundle)
        {
            if (player is not CatanPlayer catanPlayer) return false;
            return catanPlayer.Resources.CanAfford(bundle);
        }

        private static CatanResourceType? ResolveResourceType(GameCore.Resources.IResource resource)
        {
            if (resource == CatanResources.Wood) return CatanResourceType.Wood;
            if (resource == CatanResources.Brick) return CatanResourceType.Brick;
            if (resource == CatanResources.Sheep) return CatanResourceType.Sheep;
            if (resource == CatanResources.Wheat) return CatanResourceType.Wheat;
            if (resource == CatanResources.Ore) return CatanResourceType.Ore;
            return null;
        }
    }
}
