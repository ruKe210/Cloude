using STS.Cards;
using STS.Core;
using STS.Entities;

namespace STS.Battle
{
    public class TurnManager
    {
        public void StartFirstPlayerTurn(BattleContext context)
        {
            context.TurnNumber = 1;
            context.Player.RefillEnergy();
            context.Player.DrawCards(5);
            context.Phase = BattlePhase.PlayerTurn;
        }

        public void StartNewPlayerTurn(BattleContext context)
        {
            context.TurnNumber++;
            context.Player.ClearBlock();
            context.Enemy.ClearBlock();
            Powers.PowerSystem.DecrementRoundEndPowers(context);
            context.Player.RefillEnergy();
            context.Player.DrawCards(5);
            context.Phase = BattlePhase.PlayerTurn;
        }
    }
}
