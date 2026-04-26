using UnityEngine;

namespace Attack
{
    /// <summary>
    /// Per-enemy runtime state for dash attacks.
    /// Stored on the attacker GameObject to avoid shared ScriptableObject state.
    /// </summary>
    public class DashAttackRuntime : MonoBehaviour
    {
        public bool IsActive { get; private set; }
        public bool HasHit { get; set; }
        public Vector2 Direction { get; private set; }
        public float RemainingDistance { get; set; }

        public void Begin(Vector2 direction, float travelDistance)
        {
            IsActive = true;
            HasHit = false;
            Direction = direction.normalized;
            RemainingDistance = Mathf.Max(0f, travelDistance);
        }

        public void ResetState()
        {
            IsActive = false;
            HasHit = false;
            Direction = Vector2.zero;
            RemainingDistance = 0f;
        }

        private void OnDisable()
        {
            // Ensure pooled enemies don't keep stale dash state between spawns.
            ResetState();
        }
    }
}
