using STS.Battle;
using STS.Data;
using STS.Entities;

namespace STS.Actions
{
    public class DamageAction : ActionBase
    {
        private readonly CombatEntity _target;
        private readonly int _baseDamage;
        private readonly CombatEntity _source;
        private readonly DamageType _damageType;

        public DamageAction(CombatEntity target, int baseDamage, CombatEntity source, DamageType damageType = DamageType.Normal)
        {
            _target = target;
            _baseDamage = baseDamage;
            _source = source;
            _damageType = damageType;
        }

        protected override void OnStart()
        {
            var damage = DamageCalculator.ResolveDamage(_baseDamage, _source, _target, _damageType);
            var hpLoss = _target.TakeDamage(damage);
            Manager.OnDamageApplied(_target, hpLoss, _source);
            Manager.AddToBottom(new WaitAction(0.15f));
            SetDone();
        }
    }
}
