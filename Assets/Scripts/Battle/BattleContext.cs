using STS.Core;
using STS.Entities;

namespace STS.Battle
{
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
