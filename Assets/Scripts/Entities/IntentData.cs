namespace STS.Entities
{
    public readonly struct IntentData
    {
        public STS.Data.IntentType Type { get; }
        public int PrimaryValue { get; }
        public int SecondaryValue { get; }

        public IntentData(STS.Data.IntentType type, int primaryValue, int secondaryValue = 0)
        {
            Type = type;
            PrimaryValue = primaryValue;
            SecondaryValue = secondaryValue;
        }

        public string ToDisplayString()
        {
            return Type switch
            {
                STS.Data.IntentType.Attack => $"攻击 {PrimaryValue}",
                STS.Data.IntentType.Defend => $"防御 {PrimaryValue}",
                STS.Data.IntentType.Buff => $"强化 +{PrimaryValue}",
                STS.Data.IntentType.DefendBuff => $"防御+强化 {SecondaryValue}/{PrimaryValue}",
                STS.Data.IntentType.AttackDefend => $"攻+防 {PrimaryValue}/{SecondaryValue}",
                _ => "未知"
            };
        }
    }
}
