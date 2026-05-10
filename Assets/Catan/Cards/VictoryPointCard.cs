using GameCore.Cards;

namespace Catan
{
    public class VictoryPointCard : DevelopmentCard
    {
        public int Points => 1;
        public override string CardId => "victory_point";
        public override string DisplayName => "Victory Point";
        public override bool IsPlayable(IGameContext context) => false;
        public override void OnPlay(IGameContext context) { }
    }
}
