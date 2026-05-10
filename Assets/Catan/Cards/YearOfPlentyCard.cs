using GameCore.Cards;

namespace Catan
{
    public class YearOfPlentyCard : DevelopmentCard
    {
        public override string CardId => "year_of_plenty";
        public override string DisplayName => "Year of Plenty";
        public override bool IsPlayable(IGameContext context) => throw new System.NotImplementedException();
        public override void OnPlay(IGameContext context) => throw new System.NotImplementedException();
    }
}
