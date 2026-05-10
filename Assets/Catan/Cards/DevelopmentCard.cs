using GameCore.Cards;

namespace Catan
{
    public abstract class DevelopmentCard : ICard
    {
        public int TurnPurchased { get; set; }
        public abstract string CardId { get; }
        public abstract string DisplayName { get; }
        public abstract bool IsPlayable(IGameContext context);
        public abstract void OnPlay(IGameContext context);
    }
}
