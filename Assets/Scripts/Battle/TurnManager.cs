using STS.Cards;
using STS.Core;
using STS.Entities;

namespace STS.Battle
{
    /// <summary>
    /// 回合生命周期：首回合与后续回合的抽牌、回能、清格挡、Buff 递减。
    /// </summary>
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
