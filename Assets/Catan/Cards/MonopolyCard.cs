using GameCore.Cards;

namespace Catan
{
    public class MonopolyCard : DevelopmentCard
    {
        public override string CardId => "monopoly";
        public override string DisplayName => "Monopoly";
        public override bool IsPlayable(IGameContext context) => throw new System.NotImplementedException();
        public override void OnPlay(IGameContext context) => throw new System.NotImplementedException();
    }
}
