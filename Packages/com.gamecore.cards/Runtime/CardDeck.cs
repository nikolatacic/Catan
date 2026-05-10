using System.Collections.Generic;
using GameCore.Events;

namespace GameCore.Cards
{
    public class CardDeck<T> where T : ICard
    {
        private readonly List<T> _cards = new();
        public int Count => _cards.Count;

        public void AddToBottom(T card) => _cards.Add(card);
        public void AddToTop(T card) => _cards.Insert(0, card);

        public T Draw()
        {
            if (_cards.Count == 0) { EventBus.Publish(new DeckEmptyEvent()); return default; }
            var card = _cards[0];
            _cards.RemoveAt(0);
            EventBus.Publish(new CardDrawnEvent<T> { Card = card });
            return card;
        }

        public void Shuffle()
        {
            var rng = new System.Random();
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }
    }
}
