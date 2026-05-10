using System.Collections.Generic;
using GameCore.Events;

namespace GameCore.Cards
{
    public class CardHand<T> where T : ICard
    {
        private readonly List<T> _cards = new();
        public IReadOnlyList<T> Cards => _cards;

        public void Add(T card) => _cards.Add(card);
        public void Remove(T card) => _cards.Remove(card);

        public void Play(T card, IGameContext context)
        {
            if (!card.IsPlayable(context)) return;
            card.OnPlay(context);
            _cards.Remove(card);
            EventBus.Publish(new CardPlayedEvent<T> { Card = card });
        }
    }
}
