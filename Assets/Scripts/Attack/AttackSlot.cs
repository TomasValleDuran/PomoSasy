using UnityEngine;

namespace Attack
{
    [System.Serializable]
    public class AttackSlot
    {
        [SerializeField] private AttackData attackData;

        private float _cooldownTimer;
        private Transform _owner;
        private GameObject _visualInstance;

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

            _cooldownTimer -= baseContext.deltaTime;
            if (_cooldownTimer > 0f)
                return;

            AttackContext slotContext = new AttackContext(
                _owner,
                baseContext.target,
                attackData.Damage,
                attackData.AttackRange,
                baseContext.deltaTime
            );

            bool finished = behavior.Execute(slotContext);
            if (finished)
                _cooldownTimer = attackData.Cooldown;
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
            _cooldownTimer = 0f;
        }
    }
}
