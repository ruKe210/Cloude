using STS.Battle;

namespace STS.Actions
{
    public interface ICombatAction
    {
        bool IsDone { get; }
        void Start(CombatActionManager manager, BattleContext context);
        void Tick(float deltaTime);
    }
}
