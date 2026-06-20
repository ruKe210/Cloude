using STS.Battle;

namespace STS.Actions
{
    public abstract class ActionBase : ICombatAction
    {
        protected CombatActionManager Manager;
        protected BattleContext Context;

        public bool IsDone { get; protected set; }

        public void Start(CombatActionManager manager, BattleContext context)
        {
            Manager = manager;
            Context = context;
            OnStart();
        }

        public virtual void Tick(float deltaTime)
        {
        }

        protected abstract void OnStart();

        protected void SetDone() => IsDone = true;
    }
}
