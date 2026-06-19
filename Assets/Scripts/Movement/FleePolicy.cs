using UnityEngine;

namespace Movement
{
    /// <summary>Runs directly away from the target. Pair with a ranged attack for a "kiting mage".</summary>
    [CreateAssetMenu(fileName = "FleePolicy", menuName = "AI/Movement/Flee")]
    public class FleePolicy : MovementPolicy
    {
        public override Vector2 GetDirection(Transform self, Transform target)
        {
            if (target == null)
                return Vector2.zero;

            return ((Vector2)self.position - (Vector2)target.position).normalized;
        }
    }
}
