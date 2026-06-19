using System.Collections.Generic;
using Health;
using UnityEngine;

namespace Attack
{
    [CreateAssetMenu(fileName = "AuraAttack", menuName = "Scriptable Objects/Attacks/Aura")]
    public class AuraAttackBehavior : AttackBehavior
    {
        [SerializeField] private float fallbackRadius = 1.5f;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private GameObject visualPrefab;

        public override bool HasTargetInRange(in AttackContext ctx) =>
            TargetWithinRange(ctx, ctx.range > 0f ? ctx.range : fallbackRadius);

        public override bool Execute(in AttackContext ctx)
        {
            return ExecuteWithResult(ctx).Finished;
        }

        public override AttackExecutionResult ExecuteWithResult(in AttackContext ctx)
        {
            if (ctx.attacker == null)
                return new AttackExecutionResult(true, false);

            float radius = ctx.range > 0f ? ctx.range : fallbackRadius;
            Collider2D[] hits = Physics2D.OverlapCircleAll(ctx.attacker.position, radius, hitMask);
            var damaged = new HashSet<GameObject>();
            Transform attackerRoot = ctx.attacker.root;
            bool hitAny = false;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
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
