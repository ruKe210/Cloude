using System;
using System.Collections.Generic;
using STS.Actions;
using STS.Cards;
using STS.Core;
using STS.Data;
using STS.Entities;

namespace STS.Battle
{
    /// <summary>
    /// 战斗动作队列调度器：卡牌出牌、敌人回合、等待动画等均入队顺序执行。
    /// 每帧 ProcessTick 推进当前 Action；IsBusy 时 BattleManager 拒绝新输入。
    /// </summary>
    public class CombatActionManager
    {
        /// <summary>通用动作队列（弃牌、敌人 AI、等待等）。</summary>
        private readonly Queue<ICombatAction> _actions = new Queue<ICombatAction>();
        /// <summary>玩家出牌专用队列，与 _actions 交替消费。</summary>
        private readonly Queue<CardQueueItem> _cardQueue = new Queue<CardQueueItem>();

        private BattleContext _context;
        private TurnManager _turnManager;
        private BattleManager _battleManager;
        private ICombatAction _currentAction;
        /// <summary>玩家已点「结束回合」，敌人阶段进行中。</summary>
        private bool _turnHasEnded;

        public bool IsBusy => _currentAction != null || _actions.Count > 0 || _cardQueue.Count > 0 || _turnHasEnded;
        public bool TurnHasEnded => _turnHasEnded;

        public event Action StateChanged;
        public event Action<CombatEntity, int, CombatEntity> DamageDealt;
        public event Action<BattleOutcome> BattleEnded;

        public void Initialize(BattleContext context, TurnManager turnManager, BattleManager battleManager)
        {
            _context = context;
            _turnManager = turnManager;
            _battleManager = battleManager;
        }

        public void NotifyStateChanged() => StateChanged?.Invoke();

        public void AddToBottom(ICombatAction action) => _actions.Enqueue(action);

        public void EnqueueCard(CardInstance card, CombatEntity target)
        {
            _cardQueue.Enqueue(new CardQueueItem(card, target));
        }

        /// <summary>结束玩家回合：弃手牌 → 短暂等待 → 敌人行动。</summary>
        public void BeginEnemyTurn()
        {
            _turnHasEnded = true;
            AddToBottom(new DiscardHandAction());
            AddToBottom(new WaitAction(0.2f));
            AddToBottom(new EnemyTurnAction());
        }

        /// <summary>由 BattleSceneController 每帧调用，驱动队列推进。</summary>
        public void ProcessTick(float deltaTime)
        {
            if (_context.IsBattleOver)
            {
                return;
            }

            if (_currentAction != null)
            {
                _currentAction.Tick(deltaTime);
                if (!_currentAction.IsDone)
                {
                    return;
                }

                _currentAction = null;
            }

            while (_currentAction == null)
            {
                // 优先处理显式 Action（敌人回合、动画等待等）
                if (_actions.Count > 0)
                {
                    _currentAction = _actions.Dequeue();
                    _currentAction.Start(this, _context);
                    if (!_currentAction.IsDone)
                    {
                        return;
                    }

                    _currentAction = null;
                    continue;
                }

                // 再处理玩家出牌
                if (_cardQueue.Count > 0)
                {
                    ProcessCardQueue();
                    continue;
                }

                if (_turnHasEnded)
                {
                    return;
                }

                // 动画阶段结束，回到玩家回合
                if (_context.Phase == BattlePhase.Animating)
                {
                    _context.Phase = BattlePhase.PlayerTurn;
                    NotifyStateChanged();
                }

                return;
            }
        }

        private void ProcessCardQueue()
        {
            var item = _cardQueue.Dequeue();
            if (!_battleManager.CanPlayQueuedCard(item.Card, item.Target))
            {
                NotifyStateChanged();
                return;
            }

            item.Card.Play(_context, this, item.Target);
            AddToBottom(new UseCardAction(item.Card));
            CheckBattleEnd();
            NotifyStateChanged();
        }

        internal void OnDamageApplied(CombatEntity target, int damage, CombatEntity source)
        {
            DamageDealt?.Invoke(target, damage, source);
            CheckBattleEnd();
            NotifyStateChanged();
        }

        /// <summary>敌人回合 Action 链结束后，开启新玩家回合。</summary>
        internal void CompleteEnemyTurn()
        {
            _turnHasEnded = false;
            if (_context.IsBattleOver)
            {
                return;
            }

            _turnManager.StartNewPlayerTurn(_context);
            NotifyStateChanged();
        }

        internal void CheckBattleEnd()
        {
            if (_context.Outcome != BattleOutcome.InProgress)
            {
                return;
            }

            if (_context.Player.IsDead)
            {
                EndBattle(BattleOutcome.Defeat);
                return;
            }

            if (_context.Enemy.IsDead)
            {
                EndBattle(BattleOutcome.Victory);
            }
        }

        private void EndBattle(BattleOutcome outcome)
        {
            _context.Outcome = outcome;
            _context.Phase = BattlePhase.BattleEnd;
            _actions.Clear();
            _cardQueue.Clear();
            _currentAction = null;
            _turnHasEnded = false;
            BattleEnded?.Invoke(outcome);
            NotifyStateChanged();
        }
    }
}
