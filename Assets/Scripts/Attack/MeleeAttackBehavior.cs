using Health;
using UnityEngine;

namespace Attack
{
    [CreateAssetMenu(fileName = "MeleeAttack", menuName = "Scriptable Objects/Attacks/Melee")]
    public class MeleeAttackBehavior : AttackBehavior
    {
        [SerializeField] private GameObject visualPrefab;

        public override bool Execute(in AttackContext ctx)
        {
            return ExecuteWithResult(ctx).Finished;
        }

        public override AttackExecutionResult ExecuteWithResult(in AttackContext ctx)
        {
            if (ctx.attacker == null || ctx.target == null)
                return new AttackExecutionResult(true, false);

            if (ctx.range > 0f &&
                Vector2.Distance(ctx.attacker.position, ctx.target.position) > ctx.range)
                return new AttackExecutionResult(true, false);

            IDamageable damageable = ctx.target.GetComponentInParent<IDamageable>();
            if (damageable == null)
                return new AttackExecutionResult(true, false);

            damageable.TakeDamage(ctx.damage);
            return new AttackExecutionResult(true, true);
        }

        public override GameObject CreateVisual(Transform attacker) => visualPrefab;
    }
}
