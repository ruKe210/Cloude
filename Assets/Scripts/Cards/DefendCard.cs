using STS.Actions;
using STS.Battle;
using STS.Data;
using STS.Entities;

namespace STS.Cards
{
    public class DefendCard : CardInstance
    {
        private const int BaseBlock = 5;

        public DefendCard() : base("Defend", 1, CardType.Skill, CardTargetType.Self)
        {
        }

        public override void Play(BattleContext context, CombatActionManager actions, CombatEntity target)
        {
            actions.AddToBottom(new GainBlockAction(context.Player, BaseBlock));
        }

        public override int GetDisplayBlock() => BaseBlock;
    }
}
