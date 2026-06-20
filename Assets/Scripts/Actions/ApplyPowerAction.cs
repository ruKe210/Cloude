using STS.Entities;
using STS.Powers;

namespace STS.Actions
{
    public class ApplyPowerAction : ActionBase
    {
        private readonly CombatEntity _target;
        private readonly PowerType _powerType;
        private readonly int _amount;
        private readonly bool _decrementOnRoundEnd;

        public ApplyPowerAction(
            CombatEntity target,
            PowerType powerType,
            int amount,
            bool decrementOnRoundEnd = true)
        {
            _target = target;
            _powerType = powerType;
            _amount = amount;
            _decrementOnRoundEnd = decrementOnRoundEnd;
        }

        protected override void OnStart()
        {
            PowerSystem.Apply(_target, _powerType, _amount, _decrementOnRoundEnd);
            Manager.NotifyStateChanged();
            SetDone();
        }
    }
}
