using Attack;
using UnityEngine;

namespace Upgrades
{
    public enum UpgradeKind
    {
        Passive,
        Attack
    }

    public enum PassiveUpgradeEffectType
    {
        MoveSpeedMultiplier,
        XpGainMultiplier,
        MaxHealthMultiplier
    }

    [CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "Scriptable Objects/Upgrades/UpgradeDefinition")]
    public class UpgradeDefinition : ScriptableObject
    {
        [Header("Display")]
        [SerializeField] private string upgradeId;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] [Min(1)] private int maxLevel = 5;

        [Header("Type")]
        [SerializeField] private UpgradeKind upgradeKind = UpgradeKind.Passive;

        [Header("Passive Effect")]
        [SerializeField] private PassiveUpgradeEffectType passiveEffectType = PassiveUpgradeEffectType.MoveSpeedMultiplier;
        [SerializeField] [Min(0.01f)] private float passiveMultiplierPerLevel = 1.1f;

        [Header("Attack Effect")]
        [SerializeField] private AttackData targetAttack;
        [SerializeField] [Min(0.01f)] private float attackDamageMultiplierPerLevel = 1.15f;
        [SerializeField] [Min(0.01f)] private float attackCooldownMultiplierPerLevel = 0.93f;
        [SerializeField] [Min(0.01f)] private float attackRangeMultiplierPerLevel = 1.08f;

        public string UpgradeId => string.IsNullOrWhiteSpace(upgradeId) ? name : upgradeId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public int MaxLevel => maxLevel;
        public UpgradeKind Kind => upgradeKind;
        public PassiveUpgradeEffectType PassiveEffectType => passiveEffectType;
        public float PassiveMultiplierPerLevel => passiveMultiplierPerLevel;
        public AttackData TargetAttack => targetAttack;
        public float AttackDamageMultiplierPerLevel => attackDamageMultiplierPerLevel;
        public float AttackCooldownMultiplierPerLevel => attackCooldownMultiplierPerLevel;
        public float AttackRangeMultiplierPerLevel => attackRangeMultiplierPerLevel;
    }
}
