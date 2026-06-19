using System;
using System.Collections.Generic;
using UnityEngine;
using Upgrades;

namespace Attack
{
    [System.Serializable]
    public class AttackSlot
    {
        [SerializeField] private AttackData attackData;

        /// <summary>Raised when this slot fires its attack. Drives the player's attack animation.</summary>
        public event Action OnAttackPerformed;

        private float _cooldownTimer;
        private Transform _owner;
        private GameObject _visualInstance;
        private AudioSource _audioSource;
        private PlayerUpgradeModifiers _upgradeModifiers;
        private IAttackPulse[] _pulseTargets;

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
            {
                _owner.TryGetComponent(out _upgradeModifiers);
                _audioSource = null;
                if (!_owner.TryGetComponent(out _audioSource))
                    _audioSource = _owner.gameObject.AddComponent<AudioSource>();
            }

            AttackBehavior behavior = attackData?.AttackBehavior;
            if (behavior == null || owner == null)
                return;

            behavior.OnEquip(owner);

            GameObject visualPrefab = behavior.CreateVisual(owner);
            if (visualPrefab == null)
                return;

            _visualInstance = UnityEngine.Object.Instantiate(visualPrefab, owner.position, Quaternion.identity, owner);
            MonoBehaviour[] behaviours = _visualInstance.GetComponentsInChildren<MonoBehaviour>(true);
            List<IAttackPulse> pulseTargets = null;
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IAttackVisual attackVisual)
                    attackVisual.Initialize(attackData, behavior);
                if (behaviours[i] is IAttackPulse pulse)
                    (pulseTargets ??= new List<IAttackPulse>()).Add(pulse);
            }
            _pulseTargets = pulseTargets?.ToArray();
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
                baseContext.deltaTime,
                attackData.AttackSfx
            );

            AttackExecutionResult result = behavior.ExecuteWithResult(slotContext);
            if (result.PlaySfx)
                AttackAudioPlayer.Play(_audioSource, attackData);
                _cooldownTimer = effectiveCooldown;
            }
            if (result.Finished)
                            _cooldownTimer = effectiveCooldown;

            // No valid target in range: stay idle (no fire, no audio, no attack animation) and keep
                        // the cooldown ready so the attack lands the instant an enemy steps into range.
                        if (!behavior.HasTargetInRange(slotContext))
                        {
                            _cooldownTimer = 0f;
                            return;
                        }

                        bool finished = behavior.Execute(slotContext);
                        if (finished)
                        {
                            AttackAudioPlayer.Play(_audioSource, attackData);
                            _cooldownTimer = effectiveCooldown;
                            OnAttackPerformed?.Invoke();

                            if (_pulseTargets != null)
                            {
                                for (int i = 0; i < _pulseTargets.Length; i++)
                                    _pulseTargets[i].Pulse();
                            }
                        }
        }

        public void Unequip()
        {
            AttackBehavior behavior = attackData?.AttackBehavior;
            if (behavior != null && _owner != null)
                behavior.OnUnequip(_owner);

            if (_visualInstance != null)
                UnityEngine.Object.Destroy(_visualInstance);

            _visualInstance = null;
            _pulseTargets = null;
            _owner = null;
            _audioSource = null;
            _upgradeModifiers = null;
            _cooldownTimer = 0f;
        }
    }
}
