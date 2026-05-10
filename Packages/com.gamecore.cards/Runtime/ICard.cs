namespace GameCore.Cards
{
    public interface ICard
    {
        string CardId { get; }
        string DisplayName { get; }
        bool IsPlayable(IGameContext context);
        void OnPlay(IGameContext context);
    }
}
