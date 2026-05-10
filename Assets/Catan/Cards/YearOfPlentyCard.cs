using GameCore.Cards;
using GameCore.Events;
using GameCore.Resources;

namespace Catan
{
    public struct YearOfPlentyPlayedEvent : IGameEvent
    {
        public GameCore.Player.IPlayer Player;
        public ResourceBundle Granted;
    }

    public class YearOfPlentyCard : DevelopmentCard
    {
        // Set these before calling OnPlay to specify which two resources to receive.
        public CatanResourceType FirstChosenResource { get; set; }
        public CatanResourceType SecondChosenResource { get; set; }

        public override string CardId => "year_of_plenty";
        public override string DisplayName => "Year of Plenty";

        public override bool IsPlayable(IGameContext context)
        {
            if (context is not CatanGameContext catanContext) return false;
            if (TurnPurchased == catanContext.TurnManager.TurnNumber) return false;

            var phase = catanContext.TurnManager.CurrentCatanPhase;
            return phase == CatanTurnPhase.Trading || phase == CatanTurnPhase.Building;
        }

        public override void OnPlay(IGameContext context)
        {
            var activePlayer = (CatanPlayer)context.ActivePlayer;

            var granted = new ResourceBundle()
                .Add(CatanResources.Get(FirstChosenResource), 1)
                .Add(CatanResources.Get(SecondChosenResource), 1);

            activePlayer.Resources.TryAdd(granted);

            EventBus.Publish(new YearOfPlentyPlayedEvent
            {
                Player = context.ActivePlayer,
                Granted = granted
            });
        }
    }
}
