using UnityEngine;

namespace Movement
{
    /// <summary>
    /// Keeps a preferred distance from the target: approaches when too far, backs off when too
    /// close, holds still inside the tolerance band. Pair with a ranged attack (and an
    /// <c>attackRange</c> >= <see cref="desiredDistance"/>) for an "archer" that shoots from afar.
    /// </summary>
    [CreateAssetMenu(fileName = "MaintainDistancePolicy", menuName = "AI/Movement/MaintainDistance")]
    public class MaintainDistancePolicy : MovementPolicy
    {
        [SerializeField] [Min(0f)] private float desiredDistance = 6f;
        [SerializeField] [Min(0f)] private float tolerance = 0.5f;

        public override Vector2 GetDirection(Transform self, Transform target)
        {
            if (target == null)
                return Vector2.zero;

            float distance = Vector2.Distance(self.position, target.position);

            if (distance > desiredDistance + tolerance)
                return ((Vector2)target.position - (Vector2)self.position).normalized; // approach

            if (distance < desiredDistance - tolerance)
                return ((Vector2)self.position - (Vector2)target.position).normalized; // back off

            return Vector2.zero; // hold position
        }
    }
}
