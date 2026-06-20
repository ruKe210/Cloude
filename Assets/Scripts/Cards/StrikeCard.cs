using STS.Actions;
using STS.Battle;
using STS.Data;
using STS.Entities;

namespace STS.Cards
{
    public class StrikeCard : CardInstance
    {
        private const int BaseDamage = 6;

        public StrikeCard() : base("Strike", 1, CardType.Attack, CardTargetType.Enemy)
        {
        }

        public override void Play(BattleContext context, CombatActionManager actions, CombatEntity target)
        {
            actions.AddToBottom(new DamageAction(target, BaseDamage, context.Player));
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
