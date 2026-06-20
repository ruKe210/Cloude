using STS.Battle;
using STS.Data;
using STS.Entities;

namespace STS.Cards
{
    public abstract class CardInstance
    {
        public string Name { get; }
        public int Cost { get; }
        public CardType Type { get; }
        public CardTargetType TargetType { get; }

        protected CardInstance(string name, int cost, CardType type, CardTargetType targetType)
        {
            Name = name;
            Cost = cost;
            Type = type;
            TargetType = targetType;
        }

        public abstract void Play(BattleContext context, CombatActionManager actions, CombatEntity target);

        public virtual int GetDisplayDamage(BattleContext context, CombatEntity target) => 0;

        public virtual int GetDisplayBlock() => 0;
    }
}
