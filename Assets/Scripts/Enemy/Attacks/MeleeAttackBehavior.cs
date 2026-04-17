using UnityEngine;

namespace Enemy.Attacks
{
    [CreateAssetMenu(fileName = "MeleeAttack", menuName = "Scriptable Objects/Attacks/Melee")]
    public class MeleeAttackBehavior : AttackBehavior
    {
        public override bool Execute(Transform enemy, Vector2 target, float damage)
        {
            // TODO: deal damage to player
            Debug.Log($"Skeleton swings for {damage} damage!");
            return true;
        }
    }
}
