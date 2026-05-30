using Attack;
using UnityEngine;

namespace Upgrades
{
    public enum AttackUpgradeStat
    {
        Damage,
        Cooldown,
        Range
    }

    [DisallowMultipleComponent]
    public class PlayerUpgradeModifiers : MonoBehaviour
    {
        private struct AttackModifierSet
        {
            public float DamageMultiplier;
            public float CooldownMultiplier;
            public float RangeMultiplier;
        }

        private readonly System.Collections.Generic.Dictionary<AttackData, AttackModifierSet> _attackModifiers = new();

        public float MoveSpeedMultiplier { get; private set; } = 1f;
        public float XpGainMultiplier { get; private set; } = 1f;

        private void Awake()
        {
            ResetAll();
        }

        public void ResetAll()
        {
            MoveSpeedMultiplier = 1f;
            XpGainMultiplier = 1f;
            _attackModifiers.Clear();
        }

        private static AttackModifierSet CreateDefaults()
        {
            return new AttackModifierSet
            {
                DamageMultiplier = 1f,
                CooldownMultiplier = 1f,
                RangeMultiplier = 1f
            };
        }

        public void MultiplyMoveSpeed(float multiplier)
        {
            MoveSpeedMultiplier *= Mathf.Max(0.01f, multiplier);
        }

        public void MultiplyXpGain(float multiplier)
        {
            XpGainMultiplier *= Mathf.Max(0.01f, multiplier);
        }

        public void MultiplyAttackStat(AttackData attackData, AttackUpgradeStat stat, float multiplier)
        {
            if (attackData == null)
                return;

            float safeMultiplier = Mathf.Max(0.01f, multiplier);
            AttackModifierSet set = GetSet(attackData);

            switch (stat)
            {
                case AttackUpgradeStat.Damage:
                    set.DamageMultiplier *= safeMultiplier;
                    break;
                case AttackUpgradeStat.Cooldown:
                    set.CooldownMultiplier *= safeMultiplier;
                    break;
                case AttackUpgradeStat.Range:
                    set.RangeMultiplier *= safeMultiplier;
                    break;
            }

            SetSet(attackData, set);
        }

        public float GetDamageMultiplier(AttackData attackData)
        {
            return GetSet(attackData).DamageMultiplier;
        }

        public float GetCooldownMultiplier(AttackData attackData)
        {
            return GetSet(attackData).CooldownMultiplier;
        }

        public float GetRangeMultiplier(AttackData attackData)
        {
            return GetSet(attackData).RangeMultiplier;
        }

        private AttackModifierSet GetSet(AttackData attackData)
        {
            if (attackData == null)
                return CreateDefaults();
            return _attackModifiers.TryGetValue(attackData, out AttackModifierSet set) ? set : CreateDefaults();
        }

        private void SetSet(AttackData attackData, AttackModifierSet set)
        {
            if (attackData == null)
                return;
            _attackModifiers[attackData] = set;
        }
    }
}
