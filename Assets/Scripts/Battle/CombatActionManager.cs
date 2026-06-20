using System;
using System.Collections.Generic;
using STS.Actions;
using STS.Cards;
using STS.Core;
using STS.Data;
using STS.Entities;

namespace STS.Battle
{
    public class CombatActionManager
    {
        private readonly Queue<ICombatAction> _actions = new Queue<ICombatAction>();
        private readonly Queue<CardQueueItem> _cardQueue = new Queue<CardQueueItem>();

        private BattleContext _context;
        private TurnManager _turnManager;
        private BattleManager _battleManager;
        private ICombatAction _currentAction;
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

        public void BeginEnemyTurn()
        {
            _turnHasEnded = true;
            AddToBottom(new DiscardHandAction());
            AddToBottom(new WaitAction(0.2f));
            AddToBottom(new EnemyTurnAction());
        }

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

                if (_cardQueue.Count > 0)
                {
                    ProcessCardQueue();
                    continue;
                }

                if (_turnHasEnded)
                {
                    return;
                }

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
