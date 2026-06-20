using STS.Entities;

namespace STS.Actions
{
    public class GainBlockAction : ActionBase
    {
        private readonly CombatEntity _target;
        private readonly int _amount;

        public GainBlockAction(CombatEntity target, int amount)
        {
            _target = target;
            _amount = amount;
        }

        protected override void OnStart()
        {
            _target.GainBlock(_amount);
            Manager.NotifyStateChanged();
            SetDone();
        }
    }
}
