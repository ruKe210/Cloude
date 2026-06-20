using System;
using System.Collections.Generic;
using STS.Core;
using STS.Powers;

namespace STS.Entities
{
    public abstract class CombatEntity : IBattleEntity
    {
        private readonly List<PowerInstance> _powers = new List<PowerInstance>();

        public EntityId Id { get; }
        public string DisplayName { get; }
        public int CurrentHp { get; protected set; }
        public int MaxHp { get; }
        public int CurrentBlock { get; protected set; }
        public bool IsDead => CurrentHp <= 0;
        public IReadOnlyList<PowerInstance> Powers => _powers;

        protected CombatEntity(string displayName, int maxHp)
        {
            Id = EntityId.New();
            DisplayName = displayName;
            MaxHp = maxHp;
            CurrentHp = maxHp;
        }

        public void GainBlock(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            CurrentBlock += amount;
        }

        public int TakeDamage(int damage)
        {
            if (damage <= 0)
            {
                return 0;
            }

            var blocked = Math.Min(CurrentBlock, damage);
            CurrentBlock -= blocked;
            var hpLoss = damage - blocked;
            if (hpLoss > 0)
            {
                CurrentHp = Math.Max(0, CurrentHp - hpLoss);
            }

            return hpLoss;
        }

        public void ClearBlock() => CurrentBlock = 0;

        public PowerInstance GetPower(PowerType type)
        {
            for (var i = 0; i < _powers.Count; i++)
            {
                if (_powers[i].Type == type)
                {
                    return _powers[i];
                }
            }

            return null;
        }

        internal void InternalAddPower(PowerInstance power) => _powers.Add(power);

        internal void InternalRemovePower(PowerInstance power) => _powers.Remove(power);
    }
}
