using UnityEngine;

namespace Data.Attacks
{
    [CreateAssetMenu(fileName = "DashAttack", menuName = "Scriptable Objects/Attacks/Dash")]
    public class DashAttackBehavior : AttackBehavior
    {
        [SerializeField] private float dashSpeed = 10f;
        public float DashSpeed => dashSpeed;

        public override bool Execute(Transform enemy, Vector2 target, float damage)
        {
            float distanceToTarget = Vector2.Distance(enemy.position, target);

            if (distanceToTarget < 0.1f)
            {
                // TODO: deal damage to player
                Debug.Log($"Vampire strikes for {damage} damage!");
                return true;
            }

            Vector2 direction = (target - (Vector2)enemy.position).normalized;
            enemy.position += (Vector3)direction * (dashSpeed * Time.deltaTime);
            return false;
        }
    }
}
