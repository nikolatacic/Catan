using GameCore.Events;

namespace GameCore.Cards
{
    public struct CardDrawnEvent<T> : IGameEvent where T : ICard { public T Card; }
    public struct CardPlayedEvent<T> : IGameEvent where T : ICard { public T Card; }
    public struct CardDiscardedEvent<T> : IGameEvent where T : ICard { public T Card; }
    public struct DeckEmptyEvent : IGameEvent { }
}
