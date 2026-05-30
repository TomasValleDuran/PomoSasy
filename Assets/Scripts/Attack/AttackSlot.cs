using UnityEngine;
using Upgrades;

namespace Attack
{
    [System.Serializable]
    public class AttackSlot
    {
        [SerializeField] private AttackData attackData;

        private float _cooldownTimer;
        private Transform _owner;
        private GameObject _visualInstance;
        private PlayerUpgradeModifiers _upgradeModifiers;

        public AttackData AttackData => attackData;
        public bool IsEquipped => _owner != null;

        public AttackSlot(AttackData attackData)
        {
            this.attackData = attackData;
            _cooldownTimer = 0f;
        }

        public void Equip(Transform owner)
        {
            _owner = owner;
            _cooldownTimer = 0f;
            _upgradeModifiers = null;
            if (_owner != null)
                _owner.TryGetComponent(out _upgradeModifiers);

            AttackBehavior behavior = attackData?.AttackBehavior;
            if (behavior == null || owner == null)
                return;

            behavior.OnEquip(owner);

            GameObject visualPrefab = behavior.CreateVisual(owner);
            if (visualPrefab == null)
                return;

            _visualInstance = Object.Instantiate(visualPrefab, owner.position, Quaternion.identity, owner);
            MonoBehaviour[] behaviours = _visualInstance.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IAttackVisual attackVisual)
                    attackVisual.Initialize(attackData, behavior);
            }
        }

        public void Tick(in AttackContext baseContext)
        {
            AttackBehavior behavior = attackData?.AttackBehavior;
            if (behavior == null || _owner == null)
                return;

            float cooldownMultiplier = _upgradeModifiers != null ? _upgradeModifiers.GetCooldownMultiplier(attackData) : 1f;
            float damageMultiplier = _upgradeModifiers != null ? _upgradeModifiers.GetDamageMultiplier(attackData) : 1f;
            float rangeMultiplier = _upgradeModifiers != null ? _upgradeModifiers.GetRangeMultiplier(attackData) : 1f;
            float effectiveCooldown = Mathf.Max(0.01f, attackData.Cooldown * cooldownMultiplier);
            float effectiveDamage = attackData.Damage * damageMultiplier;
            float effectiveRange = attackData.AttackRange * rangeMultiplier;

            _cooldownTimer -= baseContext.deltaTime;
            if (_cooldownTimer > 0f)
                return;

            AttackContext slotContext = new AttackContext(
                _owner,
                baseContext.target,
                effectiveDamage,
                effectiveRange,
                baseContext.deltaTime
            );

            bool finished = behavior.Execute(slotContext);
            if (finished)
                _cooldownTimer = effectiveCooldown;
        }

        public void Unequip()
        {
            AttackBehavior behavior = attackData?.AttackBehavior;
            if (behavior != null && _owner != null)
                behavior.OnUnequip(_owner);

            if (_visualInstance != null)
                Object.Destroy(_visualInstance);

            _visualInstance = null;
            _owner = null;
            _upgradeModifiers = null;
            _cooldownTimer = 0f;
        }
    }
}
