using Health;
using UnityEngine;

namespace Attack
{
    [CreateAssetMenu(fileName = "MeleeAttack", menuName = "Scriptable Objects/Attacks/Melee")]
    public class MeleeAttackBehavior : AttackBehavior
    {
        public override bool Execute(Transform attacker, Transform primaryTarget, float damage, float attackRange)
        {
            if (primaryTarget == null)
                return true;

            if (attackRange > 0f &&
                Vector2.Distance(attacker.position, primaryTarget.position) > attackRange)
                return true;

            primaryTarget.GetComponentInParent<IDamageable>()?.TakeDamage(damage);
            return true;
        }
    }
}
