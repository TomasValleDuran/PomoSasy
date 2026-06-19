using Health;
using UnityEngine;
using UnityEngine.Serialization;

namespace Attack
{
    [CreateAssetMenu(fileName = "ProjectileAttack", menuName = "Scriptable Objects/Attacks/Projectile")]
    public class ProjectileAttackBehavior : AttackBehavior
    {
        [SerializeField] private float fallbackRange = 8f;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private float spawnOffset = 0.2f;
        [SerializeField] private LayerMask hitMask = ~0;
        [FormerlySerializedAs("visualPrefab")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject equippedVisualPrefab;

        public override bool Execute(in AttackContext ctx)
        {
            return ExecuteWithResult(ctx).Finished;
        }

        public override AttackExecutionResult ExecuteWithResult(in AttackContext ctx)
        {
            if (ctx.attacker == null)
                return new AttackExecutionResult(true, false);

            Vector2 direction = Vector2.right;
            if (ctx.target != null)
            {
                direction = ((Vector2)ctx.target.position - (Vector2)ctx.attacker.position).normalized;
                if (direction.sqrMagnitude < 0.0001f)
                    direction = Vector2.right;
            }

            float distance = ctx.range > 0f ? ctx.range : fallbackRange;

            if (projectilePrefab != null)
            {
                Vector3 spawnPosition = ctx.attacker.position + (Vector3)(direction * spawnOffset);
                GameObject instance = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
                ProjectileRuntime projectileRuntime = instance.GetComponent<ProjectileRuntime>();
                if (projectileRuntime == null)
                    projectileRuntime = instance.AddComponent<ProjectileRuntime>();

                projectileRuntime.Initialize(
                    ctx.attacker,
                    direction,
                    projectileSpeed,
                    ctx.damage,
                    distance,
                    hitMask,
                    ctx.attackSfx
                );

                return new AttackExecutionResult(true, false);
            }

            RaycastHit2D hit = Physics2D.Raycast(ctx.attacker.position, direction, distance, hitMask);
            if (hit.collider != null && hit.transform.root != ctx.attacker.root)
            {
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(ctx.damage);
                    return new AttackExecutionResult(true, true);
                }
            }

            return new AttackExecutionResult(true, false);
        }

        public override GameObject CreateVisual(Transform attacker) => equippedVisualPrefab;
    }
}
