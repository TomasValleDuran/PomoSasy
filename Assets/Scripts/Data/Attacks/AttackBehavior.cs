using UnityEngine;

namespace Data.Attacks
{
    public abstract class AttackBehavior : ScriptableObject
    {
        public abstract bool Execute(Transform enemy, Vector2 target, float damage);
    }
}
