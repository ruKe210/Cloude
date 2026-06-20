using System.Collections.Generic;
using STS.Powers;

namespace STS.Core
{
    public interface IBattleEntity
    {
        EntityId Id { get; }
        string DisplayName { get; }
        int CurrentHp { get; }
        int MaxHp { get; }
        int CurrentBlock { get; }
        bool IsDead { get; }
        IReadOnlyList<PowerInstance> Powers { get; }
    }
}
