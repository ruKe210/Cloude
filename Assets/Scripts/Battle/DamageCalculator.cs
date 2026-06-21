using STS.Core;
using STS.Data;
using STS.Entities;

namespace STS.Battle
{
    /// <summary>
    /// 伤害结算：力量加成、虚弱减伤、易伤增伤。Preview 与 Resolve 目前逻辑一致，供 UI 预览用。
    /// </summary>
    public static class DamageCalculator
    {
        public static int PreviewDamage(int baseDamage, CombatEntity attacker, CombatEntity target, DamageType type = DamageType.Normal)
        {
            return ResolveDamage(baseDamage, attacker, target, type);
        }

        public static int ResolveDamage(int baseDamage, CombatEntity attacker, CombatEntity target, DamageType type = DamageType.Normal)
        {
            if (type != DamageType.Normal || baseDamage <= 0)
            {
                return System.Math.Max(0, baseDamage);
            }

            var tmp = (float)baseDamage;

            foreach (var power in attacker.Powers)
            {
                switch (power.Type)
                {
                    case Powers.PowerType.Strength:
                        tmp += power.Amount;
                        break;
                    case Powers.PowerType.Weak:
                        tmp = System.MathF.Floor(tmp * 0.75f);
                        break;
                }
            }

            foreach (var power in target.Powers)
            {
                if (power.Type == Powers.PowerType.Vulnerable)
                {
                    tmp = System.MathF.Floor(tmp * 1.5f);
                }
            }

            return System.Math.Max(0, (int)tmp);
        }
    }
}
