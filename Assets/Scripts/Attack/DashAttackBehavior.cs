using Health;
using UnityEngine;

namespace Attack
{
    [CreateAssetMenu(fileName = "DashAttack", menuName = "Scriptable Objects/Attacks/Dash")]
    public class DashAttackBehavior : AttackBehavior
    {
        [SerializeField] private float dashSpeed = 10f;
        public float DashSpeed => dashSpeed;

        public override bool Execute(Transform attacker, Transform primaryTarget, float damage, float attackRange)
        {
            if (primaryTarget == null)
                return true;

            float distanceToTarget = Vector2.Distance(attacker.position, primaryTarget.position);
            float contactDistance = attackRange > 0f ? Mathf.Min(attackRange, 0.35f) : 0.1f;

            if (distanceToTarget < contactDistance)
            {
                primaryTarget.GetComponentInParent<IDamageable>()?.TakeDamage(damage);
                return true;
            }

            Vector2 direction = ((Vector2)primaryTarget.position - (Vector2)attacker.position).normalized;
            attacker.position += (Vector3)direction * (dashSpeed * Time.deltaTime);
            return false;
        }
    }
}
