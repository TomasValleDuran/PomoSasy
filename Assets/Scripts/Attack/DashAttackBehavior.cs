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

            DashAttackRuntime runtime = attacker.GetComponent<DashAttackRuntime>();
            if (runtime == null)
                runtime = attacker.gameObject.AddComponent<DashAttackRuntime>();

            if (!runtime.IsActive)
            {
                Vector2 direction = ((Vector2)primaryTarget.position - (Vector2)attacker.position).normalized;
                if (direction.sqrMagnitude < 0.0001f)
                    direction = Vector2.right;

                float travelDistance = Mathf.Max(attackRange, 0.1f);
                runtime.Begin(direction, travelDistance);
            }

            float step = dashSpeed * Time.deltaTime;
            float moveAmount = Mathf.Min(step, runtime.RemainingDistance);
            attacker.position += (Vector3)(runtime.Direction * moveAmount);
            runtime.RemainingDistance -= moveAmount;

            float contactDistance = attackRange > 0f ? Mathf.Min(attackRange, 0.35f) : 0.1f;
            if (!runtime.HasHit && Vector2.Distance(attacker.position, primaryTarget.position) <= contactDistance)
            {
                primaryTarget.GetComponentInParent<IDamageable>()?.TakeDamage(damage);
                runtime.HasHit = true;
            }

            bool finished = runtime.HasHit || runtime.RemainingDistance <= 0f;
            if (finished)
                runtime.ResetState();

            return finished;
        }
    }
}
