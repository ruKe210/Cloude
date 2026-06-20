using STS.AI;
using STS.Entities;

namespace STS.Actions
{
    public class EnemyTurnAction : ActionBase
    {
        protected override void OnStart()
        {
            var enemy = Context.Enemy;
            var move = enemy.NextMove;

            switch (move)
            {
                case JawWormAI.Chomp:
                    Manager.AddToBottom(new DamageAction(Context.Player, enemy.ChompDamage, enemy));
                    break;
                case JawWormAI.Bellow:
                    Manager.AddToBottom(new ApplyPowerAction(
                        enemy, Powers.PowerType.Strength, enemy.BellowStrength, decrementOnRoundEnd: false));
                    Manager.AddToBottom(new GainBlockAction(enemy, enemy.BellowBlock));
                    break;
                case JawWormAI.Thrash:
                    Manager.AddToBottom(new DamageAction(Context.Player, enemy.ThrashDamage, enemy));
                    Manager.AddToBottom(new GainBlockAction(enemy, enemy.ThrashBlock));
                    break;
            }

            enemy.RecordMove(move);
            enemy.RollNextMove();
            Manager.AddToBottom(new WaitAction(0.3f));
            Manager.AddToBottom(new EndEnemyTurnAction());
            SetDone();
        }
    }
}
