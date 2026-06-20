using System;
using STS.Cards;

namespace STS.Entities
{
    public class PlayerEntity : CombatEntity
    {
        public int Energy { get; private set; }
        public int MaxEnergy { get; } = 3;
        public CardPile DrawPile { get; }
        public CardPile DiscardPile { get; }
        public CardPile Hand { get; }

        public PlayerEntity(Random rng, string displayName = "Ironclad", int maxHp = 80)
            : base(displayName, maxHp)
        {
            DrawPile = new CardPile(rng);
            DiscardPile = new CardPile(rng);
            Hand = new CardPile(rng);
        }

        public void RefillEnergy() => Energy = MaxEnergy;

        public bool SpendEnergy(int cost)
        {
            if (Energy < cost)
            {
                return false;
            }

            Energy -= cost;
            return true;
        }

        public int DrawCards(int count)
        {
            var drawn = 0;
            for (var i = 0; i < count; i++)
            {
                if (DrawPile.Count == 0)
                {
                    if (DiscardPile.Count == 0)
                    {
                        break;
                    }

                    while (DiscardPile.Count > 0)
                    {
                        DrawPile.Add(DiscardPile.Draw());
                    }

                    DrawPile.Shuffle();
                }

                var card = DrawPile.Draw();
                if (card == null)
                {
                    break;
                }

                Hand.Add(card);
                drawn++;
            }

            return drawn;
        }

        public void DiscardHand()
        {
            while (Hand.Count > 0)
            {
                DiscardPile.Add(Hand.Draw());
            }
        }
    }
}
