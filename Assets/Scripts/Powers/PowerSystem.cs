using System.Collections.Generic;
using STS.Battle;
using STS.Entities;

namespace STS.Powers
{
    public static class PowerSystem
    {
        public static void Apply(CombatEntity owner, PowerType type, int amount, bool decrementOnRoundEnd)
        {
            if (amount <= 0)
            {
                return;
            }

            var existing = owner.GetPower(type);
            if (existing != null)
            {
                existing.Amount += amount;
                return;
            }

            owner.InternalAddPower(new PowerInstance(type, amount, owner, decrementOnRoundEnd));
        }

        public static void DecrementRoundEndPowers(BattleContext context)
        {
            DecrementRoundEndPowers(context.Player);
            DecrementRoundEndPowers(context.Enemy);
        }

        private static void DecrementRoundEndPowers(CombatEntity entity)
        {
            var toRemove = new List<PowerInstance>();
            foreach (var power in entity.Powers)
            {
                if (!power.DecrementOnRoundEnd)
                {
                    continue;
                }

                power.Amount--;
                if (power.Amount <= 0)
                {
                    toRemove.Add(power);
                }
            }

            for (var i = 0; i < toRemove.Count; i++)
            {
                entity.InternalRemovePower(toRemove[i]);
            }
        }
    }
}
