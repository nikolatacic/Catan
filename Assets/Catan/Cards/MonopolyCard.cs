using GameCore.Cards;
using GameCore.Events;
using GameCore.Resources;

namespace Catan
{
    public struct MonopolyPlayedEvent : IGameEvent
    {
        public GameCore.Player.IPlayer Player;
        public CatanResourceType Resource;
        public ResourceBundle TotalStolen;
    }

    public class MonopolyCard : DevelopmentCard
    {
        // Set this before calling OnPlay to specify which resource to monopolise.
        public CatanResourceType ChosenResourceType { get; set; }

        public override string CardId => "monopoly";
        public override string DisplayName => "Monopoly";

        public override bool IsPlayable(IGameContext context)
        {
            if (context is not CatanGameContext catanContext) return false;
            if (TurnPurchased == catanContext.TurnManager.TurnNumber) return false;

            var phase = catanContext.TurnManager.CurrentCatanPhase;
            return phase == CatanTurnPhase.Trading || phase == CatanTurnPhase.Building;
        }

        public override void OnPlay(IGameContext context)
        {
            var catanContext = (CatanGameContext)context;
            var activePlayer = (CatanPlayer)context.ActivePlayer;
            var targetResource = CatanResources.Get(ChosenResourceType);

            var totalStolen = new ResourceBundle();

            foreach (var player in catanContext.AllPlayers)
            {
                if (player == context.ActivePlayer) continue;
                if (player is not CatanPlayer otherPlayer) continue;

                int amount = otherPlayer.Resources.Current.Get(targetResource);
                if (amount <= 0) continue;

                var stolen = new ResourceBundle().Add(targetResource, amount);
                otherPlayer.Resources.TryRemove(stolen);
                activePlayer.Resources.TryAdd(stolen);
                totalStolen = totalStolen.Add(targetResource, amount);
            }

            EventBus.Publish(new MonopolyPlayedEvent
            {
                Player = context.ActivePlayer,
                Resource = ChosenResourceType,
                TotalStolen = totalStolen
            });
        }
    }
}
