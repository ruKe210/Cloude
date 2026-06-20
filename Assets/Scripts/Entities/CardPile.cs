using System;
using System.Collections.Generic;
using STS.Cards;

namespace STS.Entities
{
    public class CardPile
    {
        private readonly List<CardInstance> _cards = new List<CardInstance>();
        private readonly Random _rng;

        public IReadOnlyList<CardInstance> Cards => _cards;
        public int Count => _cards.Count;

        public CardPile(Random rng)
        {
            _rng = rng;
        }

        public void Add(CardInstance card) => _cards.Add(card);

        public void AddRange(IEnumerable<CardInstance> cards) => _cards.AddRange(cards);

        public CardInstance Draw()
        {
            if (_cards.Count == 0)
            {
                return null;
            }

            var card = _cards[0];
            _cards.RemoveAt(0);
            return card;
        }

        public void Shuffle()
        {
            for (var i = _cards.Count - 1; i > 0; i--)
            {
                var j = _rng.Next(i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }

        public bool Remove(CardInstance card) => _cards.Remove(card);

        public void Clear() => _cards.Clear();
    }
}
