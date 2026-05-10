using GameCore.Cards;

namespace Catan
{
    public class RoadBuildingCard : DevelopmentCard
    {
        public override string CardId => "road_building";
        public override string DisplayName => "Road Building";
        public override bool IsPlayable(IGameContext context) => throw new System.NotImplementedException();
        public override void OnPlay(IGameContext context) => throw new System.NotImplementedException();
    }
}
