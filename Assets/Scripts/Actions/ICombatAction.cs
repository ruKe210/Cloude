using STS.Battle;

namespace STS.Actions
{
    /// <summary>
    /// 战斗动作单元：入队后由 CombatActionManager 依次 Start → Tick 直到 IsDone。
    /// 用于弃牌、等待动画、敌人回合等非即时逻辑。
    /// </summary>
    public interface ICombatAction
    {
        bool IsDone { get; }
        void Start(CombatActionManager manager, BattleContext context);
        void Tick(float deltaTime);
    }
}
