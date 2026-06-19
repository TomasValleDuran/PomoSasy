using UnityEngine;

namespace Movement
{
    /// <summary>
    /// Strategy for how an enemy moves relative to its target. Returns the direction the
    /// enemy should walk this frame (normalized, or <see cref="Vector2.zero"/> to hold position).
    /// Implemented as a <see cref="ScriptableObject"/> so a single asset can be shared by many
    /// enemies and swapped in the Inspector without touching code (see ENEMY_DESIGN.md).
    /// </summary>
    public abstract class MovementPolicy : ScriptableObject
    {
        public abstract Vector2 GetDirection(Transform self, Transform target);
    }
}
