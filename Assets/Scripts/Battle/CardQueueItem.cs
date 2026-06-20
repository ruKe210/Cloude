using STS.Cards;
using STS.Entities;

namespace STS.Battle
{
    public class CardQueueItem
    {
        public CardInstance Card { get; }
        public CombatEntity Target { get; }

        public CardQueueItem(CardInstance card, CombatEntity target)
        {
            Card = card;
            Target = target;
        }
    }
}
