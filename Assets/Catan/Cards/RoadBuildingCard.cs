using GameCore.Cards;
using GameCore.Events;

namespace Catan
{
    public struct FreeRoadsGrantedEvent : IGameEvent
    {
        public GameCore.Player.IPlayer Player;
        public int Count;
    }

    public class RoadBuildingCard : DevelopmentCard
    {
        private const int FreeRoadsPerCard = 2;

        public override string CardId => "road_building";
        public override string DisplayName => "Road Building";

        public override bool IsPlayable(IGameContext context)
        {
            if (context is not CatanGameContext catanContext) return false;
            if (TurnPurchased == catanContext.TurnManager.TurnNumber) return false;

            var phase = catanContext.TurnManager.CurrentCatanPhase;
            return phase == CatanTurnPhase.Trading || phase == CatanTurnPhase.Building;
        }

        public override void OnPlay(IGameContext context)
        {
            EventBus.Publish(new FreeRoadsGrantedEvent
            {
                Player = context.ActivePlayer,
                Count = FreeRoadsPerCard
            });
        }
    }
}
