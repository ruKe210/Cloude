using STS.Actions;
using STS.Cards;
using STS.Core;
using STS.Data;
using STS.Entities;

namespace STS.Battle
{
    /// <summary>
    /// 战斗规则入口：校验出牌/结束回合，委托 TurnManager 与 CombatActionManager 执行。
    /// 纯 C# 类，Presentation 层通过 CombatActionManager 事件刷新 UI。
    /// </summary>
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

        /// <summary>校验能量与目标合法性后，将卡牌加入出牌队列。</summary>
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

        /// <summary>玩家主动结束回合，触发敌人阶段 Action 链。</summary>
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

        /// <summary>构建演示用初始牌组（5 打击 + 4 防御 + 1 重击）。</summary>
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
