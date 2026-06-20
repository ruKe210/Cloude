using System.Collections.Generic;
using STS.Data;
using STS.Entities;

namespace STS.AI
{
    public class JawWormAI
    {
        public const byte Chomp = 1;
        public const byte Bellow = 2;
        public const byte Thrash = 3;

        private readonly List<byte> _moveHistory = new List<byte>();
        private bool _firstMove = true;

        public byte GetMove(int roll, System.Random rng)
        {
            if (_firstMove)
            {
                _firstMove = false;
                return Chomp;
            }

            if (roll < 25)
            {
                if (LastMove(Chomp))
                {
                    return rng.NextDouble() < 0.5625 ? Bellow : Thrash;
                }

                return Chomp;
            }

            if (roll < 55)
            {
                if (LastTwoMoves(Thrash))
                {
                    return rng.NextDouble() < 0.357 ? Chomp : Bellow;
                }

                return Thrash;
            }

            if (LastMove(Bellow))
            {
                return rng.NextDouble() < 0.416 ? Chomp : Thrash;
            }

            return Bellow;
        }

        public void RecordMove(byte move)
        {
            _moveHistory.Add(move);
            if (_moveHistory.Count > 2)
            {
                _moveHistory.RemoveAt(0);
            }
        }

        public IntentData BuildIntent(byte move, EnemyEntity enemy)
        {
            return move switch
            {
                Chomp => new IntentData(IntentType.Attack, enemy.ChompDamage),
                Bellow => new IntentData(IntentType.DefendBuff, enemy.BellowStrength, enemy.BellowBlock),
                Thrash => new IntentData(IntentType.AttackDefend, enemy.ThrashDamage, enemy.ThrashBlock),
                _ => new IntentData(IntentType.Attack, 0)
            };
        }

        private bool LastMove(byte move)
        {
            return _moveHistory.Count > 0 && _moveHistory[_moveHistory.Count - 1] == move;
        }

        private bool LastTwoMoves(byte move)
        {
            return _moveHistory.Count >= 2
                   && _moveHistory[_moveHistory.Count - 1] == move
                   && _moveHistory[_moveHistory.Count - 2] == move;
        }
    }
}
