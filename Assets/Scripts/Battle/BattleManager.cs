using STS.Actions;
using STS.Cards;
using STS.Core;
using STS.Data;
using STS.Entities;

namespace STS.Battle
{
    public class BattleManager
    {
        public BattleContext Context { get; private set; }
        public CombatActionManager Actions { get; private set; }
        public TurnManager TurnManager { get; } = new TurnManager();

        public void Initialize(BattleContext context, CombatActionManager actions)
        {
            Context = context;
            Actions = actions;
            Actions.Initialize(context, TurnManager, this);
        }

        public void StartBattle()
        {
            BuildStarterDeck(Context.Player);
            Context.Player.DrawPile.Shuffle();
            TurnManager.StartFirstPlayerTurn(Context);
            Actions.NotifyStateChanged();
        }

        public bool RequestPlayCard(CardInstance card, CombatEntity target)
        {
            if (Context.IsBattleOver || Actions.IsBusy || Context.Phase != BattlePhase.PlayerTurn)
            {
                return false;
            }

            if (!CanUseCard(card, target))
            {
                return false;
            }

            Context.Phase = BattlePhase.Animating;
            Actions.EnqueueCard(card, target);
            Actions.NotifyStateChanged();
            return true;
        }

        public bool EndPlayerTurn()
        {
            if (Context.IsBattleOver || Actions.IsBusy || Context.Phase != BattlePhase.PlayerTurn)
            {
                return false;
            }

            Context.Phase = BattlePhase.Animating;
            Actions.BeginEnemyTurn();
            Actions.NotifyStateChanged();
            return true;
        }

        public bool CanPlayQueuedCard(CardInstance card, CombatEntity target) => CanUseCard(card, target);

        private bool CanUseCard(CardInstance card, CombatEntity target)
        {
            if (Context.Player.Energy < card.Cost)
            {
                return false;
            }

            if (card.TargetType == CardTargetType.Enemy)
            {
                return target != null && !target.IsDead && target == Context.Enemy;
            }

            return true;
        }

        private static void BuildStarterDeck(PlayerEntity player)
        {
            for (var i = 0; i < 5; i++)
            {
                player.DrawPile.Add(new StrikeCard());
            }

            for (var i = 0; i < 4; i++)
            {
                player.DrawPile.Add(new DefendCard());
            }

            player.DrawPile.Add(new BashCard());
        }
    }
}
