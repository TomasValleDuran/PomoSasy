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
        [SerializeField] private GameObject visualPrefab;

        /// <summary>Same radius <see cref="Execute"/> uses for OverlapCircle (AttackData range, or fallback).</summary>
        public float GetEffectiveRadius(float attackRangeFromAttackData) =>
            attackRangeFromAttackData > 0f ? attackRangeFromAttackData : fallbackRadius;

        /// <summary>Area attack around the attacker. <paramref name="primaryTarget"/> is ignored.</summary>
        public override bool Execute(in AttackContext ctx)
        {
            return ExecuteWithResult(ctx).Finished;
        }

        public override AttackExecutionResult ExecuteWithResult(in AttackContext ctx)
        {
            if (ctx.attacker == null)
                return new AttackExecutionResult(true, false);

            float r = GetEffectiveRadius(ctx.range);
            var attackerRoot = ctx.attacker.root;
            var hits = Physics2D.OverlapCircleAll(ctx.attacker.position, r, hitMask);
            var damaged = new HashSet<GameObject>();
            bool hitAny = false;

            foreach (Collider2D hit in hits)
            {
                if (hit.transform.root == attackerRoot)
                    continue;

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable is not MonoBehaviour mb)
                    continue;

                if (!damaged.Add(mb.gameObject))
                    continue;

                damageable.TakeDamage(ctx.damage);
                hitAny = true;
            }

            return new AttackExecutionResult(true, hitAny);
        }

        public override GameObject CreateVisual(Transform attacker) => visualPrefab;
    }
}
