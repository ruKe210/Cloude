namespace STS.Core
{
    /// <summary>战斗阶段，由 BattleManager / CombatActionManager 驱动。</summary>
    public enum BattlePhase
    {
        PlayerTurn,
        EnemyTurn,
        Animating,
        BattleEnd
    }

    /// <summary>战斗胜负结果，由 CombatActionManager.CheckBattleEnd 判定。</summary>
    public enum BattleOutcome
    {
        InProgress,
        Victory,
        Defeat
    }
}
