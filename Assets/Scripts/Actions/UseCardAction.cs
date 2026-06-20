using STS.Cards;
using STS.Entities;

namespace STS.Actions
{
    public class UseCardAction : ActionBase
    {
        private readonly CardInstance _card;

        public UseCardAction(CardInstance card)
        {
            _card = card;
        }

        protected override void OnStart()
        {
            var player = Context.Player;
            player.SpendEnergy(_card.Cost);
            player.Hand.Remove(_card);
            player.DiscardPile.Add(_card);
            Manager.CheckBattleEnd();
            SetDone();
        }
    }
}
