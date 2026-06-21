using STS.Core;
using STS.Entities;

namespace STS.Battle
{
    /// <summary>
    /// 单场战斗的运行时上下文，类似 Meta 层的 NarrativeContext。
    /// 持有双方实体、回合数、阶段与 RNG，不含 UI 或 Action 队列逻辑。
    /// </summary>
    public class BattleContext
    {
        public PlayerEntity Player { get; }
        public EnemyEntity Enemy { get; }
        public BattlePhase Phase { get; set; }
        public int TurnNumber { get; set; }
        public BattleOutcome Outcome { get; set; } = BattleOutcome.InProgress;
        public System.Random Rng { get; }

        public BattleContext(PlayerEntity player, EnemyEntity enemy, System.Random rng)
        {
            Player = player;
            Enemy = enemy;
            Rng = rng;
        }

        public bool IsBattleOver => Outcome != BattleOutcome.InProgress;
    }
}
