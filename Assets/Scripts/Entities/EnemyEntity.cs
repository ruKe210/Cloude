using System;
using STS.AI;

namespace STS.Entities
{
    public class EnemyEntity : CombatEntity
    {
        private readonly JawWormAI _ai = new JawWormAI();
        private readonly Random _rng;

        public IntentData CurrentIntent { get; private set; }
        public byte NextMove { get; private set; }

        public int ChompDamage { get; } = 11;
        public int ThrashDamage { get; } = 7;
        public int ThrashBlock { get; } = 5;
        public int BellowStrength { get; } = 3;
        public int BellowBlock { get; } = 6;

        public EnemyEntity(Random rng, int maxHp = 44)
            : base("Jaw Worm", maxHp)
        {
            _rng = rng;
            RollNextMove();
        }

        public void RollNextMove()
        {
            var roll = _rng.Next(100);
            NextMove = _ai.GetMove(roll, _rng);
            CurrentIntent = _ai.BuildIntent(NextMove, this);
        }

        public void RecordMove(byte move) => _ai.RecordMove(move);
    }
}
