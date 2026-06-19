using UnityEngine;

namespace Movement
{
    /// <summary>Walks straight at the target. This is the default behavior when an enemy has no policy.</summary>
    [CreateAssetMenu(fileName = "ChasePolicy", menuName = "AI/Movement/Chase")]
    public class ChasePolicy : MovementPolicy
    {
        public override Vector2 GetDirection(Transform self, Transform target)
        {
            if (target == null)
                return Vector2.zero;

            return ((Vector2)target.position - (Vector2)self.position).normalized;
        }
    }
}
