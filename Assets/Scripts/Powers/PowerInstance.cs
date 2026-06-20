using STS.Entities;

namespace STS.Powers
{
    public class PowerInstance
    {
        public PowerType Type { get; }
        public int Amount { get; set; }
        public CombatEntity Owner { get; }
        public bool DecrementOnRoundEnd { get; }

        public PowerInstance(PowerType type, int amount, CombatEntity owner, bool decrementOnRoundEnd)
        {
            Type = type;
            Amount = amount;
            Owner = owner;
            DecrementOnRoundEnd = decrementOnRoundEnd;
        }

        public string GetDisplayName()
        {
            return Type switch
            {
                PowerType.Strength => $"力量 {Amount}",
                PowerType.Weak => $"虚弱 {Amount}",
                PowerType.Vulnerable => $"易伤 {Amount}",
                _ => Type.ToString()
            };
        }
    }
}
