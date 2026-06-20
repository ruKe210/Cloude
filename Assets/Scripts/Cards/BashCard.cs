using STS.Actions;
using STS.Battle;
using STS.Data;
using STS.Entities;
using STS.Powers;

namespace STS.Cards
{
    public class BashCard : CardInstance
    {
        private const int BaseDamage = 8;
        private const int VulnerableAmount = 2;

        public BashCard() : base("Bash", 2, CardType.Attack, CardTargetType.Enemy)
        {
        }

        public override void Play(BattleContext context, CombatActionManager actions, CombatEntity target)
        {
            actions.AddToBottom(new DamageAction(target, BaseDamage, context.Player));
            actions.AddToBottom(new ApplyPowerAction(target, PowerType.Vulnerable, VulnerableAmount));
        }

        public override int GetDisplayDamage(BattleContext context, CombatEntity target)
        {
            if (target == null)
            {
                return BaseDamage;
            }

            return DamageCalculator.PreviewDamage(BaseDamage, context.Player, target);
        }
    }
}
