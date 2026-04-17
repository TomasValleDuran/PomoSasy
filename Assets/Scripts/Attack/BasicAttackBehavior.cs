using System.Collections.Generic;
using Health;
using UnityEngine;
using UnityEngine.Serialization;

namespace Attack
{
    [CreateAssetMenu(fileName = "BasicAttack", menuName = "Scriptable Objects/Attacks/BasicAttack")]
    public class BasicAttackBehavior : AttackBehavior
    {
        [Tooltip("Used only when AttackData attack range is 0 (legacy / optional). Prefer setting range on AttackData.")]
        [FormerlySerializedAs("radius")]
        [SerializeField] private float fallbackRadius = 1.5f;
        [SerializeField] private LayerMask hitMask = ~0;

        /// <summary>Same radius <see cref="Execute"/> uses for OverlapCircle (AttackData range, or fallback).</summary>
        public float GetEffectiveRadius(float attackRangeFromAttackData) =>
            attackRangeFromAttackData > 0f ? attackRangeFromAttackData : fallbackRadius;

        /// <summary>Area attack around the attacker. <paramref name="primaryTarget"/> is ignored.</summary>
        public override bool Execute(Transform attacker, Transform primaryTarget, float damage, float attackRange)
        {
            float r = GetEffectiveRadius(attackRange);
            var attackerRoot = attacker.root;
            var hits = Physics2D.OverlapCircleAll(attacker.position, r, hitMask);
            var damaged = new HashSet<GameObject>();

            foreach (Collider2D hit in hits)
            {
                if (hit.transform.root == attackerRoot)
                    continue;

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable is not MonoBehaviour mb)
                    continue;

                if (!damaged.Add(mb.gameObject))
                    continue;

                damageable.TakeDamage(damage);
            }

            return true;
        }
    }
}
