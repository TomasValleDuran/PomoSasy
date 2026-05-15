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
            if (ctx.attacker == null || ctx.target == null)
                return true;

            if (ctx.range > 0f &&
                Vector2.Distance(ctx.attacker.position, ctx.target.position) > ctx.range)
                return true;

            ctx.target.GetComponentInParent<IDamageable>()?.TakeDamage(ctx.damage);
            return true;
        }

        public override GameObject CreateVisual(Transform attacker) => visualPrefab;
    }
}
