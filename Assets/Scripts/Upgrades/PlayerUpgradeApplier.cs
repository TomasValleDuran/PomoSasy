using Health;
using UnityEngine;

namespace Upgrades
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerUpgradeModifiers))]
    public class PlayerUpgradeApplier : MonoBehaviour
    {
        [SerializeField] private PlayerUpgradeModifiers modifiers;
        [SerializeField] private HealthComponent healthComponent;

        private void Awake()
        {
            if (modifiers == null)
                modifiers = GetComponent<PlayerUpgradeModifiers>();
            if (healthComponent == null)
                healthComponent = GetComponent<HealthComponent>();
        }

        public bool ApplyUpgrade(UpgradeDefinition upgrade)
        {
            if (upgrade == null || modifiers == null)
                return false;

            switch (upgrade.Kind)
            {
                case UpgradeKind.Passive:
                    return ApplyPassiveUpgrade(upgrade);
                case UpgradeKind.Attack:
                    return ApplyAttackUpgrade(upgrade);
                default:
                    return false;
            }
        }

        private bool ApplyPassiveUpgrade(UpgradeDefinition upgrade)
        {
            float multiplier = upgrade.PassiveMultiplierPerLevel;
            switch (upgrade.PassiveEffectType)
            {
                case PassiveUpgradeEffectType.MoveSpeedMultiplier:
                    modifiers.MultiplyMoveSpeed(multiplier);
                    return true;
                case PassiveUpgradeEffectType.XpGainMultiplier:
                    modifiers.MultiplyXpGain(multiplier);
                    return true;
                case PassiveUpgradeEffectType.MaxHealthMultiplier:
                    if (healthComponent == null)
                        return false;

                    healthComponent.MultiplyMaxHealth(multiplier);
                    healthComponent.ResetHealth();
                    return true;
                default:
                    return false;
            }
        }

        private bool ApplyAttackUpgrade(UpgradeDefinition upgrade)
        {
            if (upgrade.TargetAttack == null)
                return false;

            modifiers.MultiplyAttackStat(upgrade.TargetAttack, AttackUpgradeStat.Damage, upgrade.AttackDamageMultiplierPerLevel);
            modifiers.MultiplyAttackStat(upgrade.TargetAttack, AttackUpgradeStat.Cooldown, upgrade.AttackCooldownMultiplierPerLevel);
            modifiers.MultiplyAttackStat(upgrade.TargetAttack, AttackUpgradeStat.Range, upgrade.AttackRangeMultiplierPerLevel);
            return true;
        }
    }
}
