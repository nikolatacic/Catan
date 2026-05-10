using GameCore.Cards;
using GameCore.Events;

namespace Catan
{
    public class KnightCard : DevelopmentCard
    {
        public override string CardId => "knight";
        public override string DisplayName => "Knight";

        public override bool IsPlayable(IGameContext context) => throw new System.NotImplementedException();

        public override void OnPlay(IGameContext context)
        {
            ((CatanPlayer)context.ActivePlayer).KnightsPlayed++;
            EventBus.Publish(new KnightPlayedEvent { Player = context.ActivePlayer });
        }
    }
}
