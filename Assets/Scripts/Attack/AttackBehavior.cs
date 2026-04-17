using UnityEngine;

namespace Attack
{
    public abstract class AttackBehavior : ScriptableObject
    {
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="damage"></param>
        /// <param name="attackRange">From <see cref="AttackData.AttackRange"/>. Melee uses it at strike time; BasicAttack ignores (uses its own radius).</param>
        public abstract bool Execute(Transform attacker, Transform target, float damage, float attackRange);
    }
}
