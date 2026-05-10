using GameCore.Cards;
using GameCore.Events;

namespace Catan
{
    public class KnightCard : DevelopmentCard
    {
        public override string CardId => "knight";
        public override string DisplayName => "Knight";

        // Knights can be played before rolling dice or during the trading/building phase,
        // but never the same turn they were purchased.
        public override bool IsPlayable(IGameContext context)
        {
            if (context is not CatanGameContext catanContext) return false;
            if (TurnPurchased == catanContext.TurnManager.TurnNumber) return false;

            var phase = catanContext.TurnManager.CurrentCatanPhase;
            return phase == CatanTurnPhase.RollDice
                || phase == CatanTurnPhase.Trading
                || phase == CatanTurnPhase.Building;
        }

        public override void OnPlay(IGameContext context)
        {
            var catanContext = (CatanGameContext)context;
            var catanPlayer = (CatanPlayer)context.ActivePlayer;

            catanPlayer.KnightsPlayed++;
            catanContext.TurnManager.RobberSystem.Activate(context.ActivePlayer);
            EventBus.Publish(new KnightPlayedEvent { Player = context.ActivePlayer });
        }
    }
}
