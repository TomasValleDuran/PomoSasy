using System.Collections.Generic;
using Health;
using UnityEngine;
using UnityEngine.Serialization;

namespace Attack
{
    [CreateAssetMenu(fileName = "BasicAttack", menuName = "Scriptable Objects/Attacks/BasicAttack")]
    public class BasicAttackBehavior : AttackBehavior
    {
        [Tooltip("Used only when AttackData attack range is 0 (legacy / optional). Prefer setting range on AttackData.")]
        [FormerlySerializedAs("radius")]
        [SerializeField] private float fallbackRadius = 1.5f;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Swing arc")]
        [Tooltip("Full angle of the swing, in degrees, centered on the line to the target. " +
                 "140 => a wide frontal swing. 180 => a semicircle (everything in front of the player). " +
                 "360 => hits all around (legacy area).")]
        [Range(0f, 360f)]
        [SerializeField] private float arcAngleDegrees = 140f;

        [Header("Swing visual (transient, spawned per attack)")]
        [Tooltip("Optional slash/swing effect instantiated each time the attack fires, oriented toward the target.")]
        [SerializeField] private GameObject swingVisualPrefab;
        [SerializeField] private float visualSpawnOffset = 0.4f;
        [SerializeField] private float visualLifetime = 0.2f;
        [SerializeField] private float visualAngleOffset = 0f;

        [Header("Equipped visual (optional)")]
        [Tooltip("Persistent visual attached to the player while equipped. Leave empty for a directional " +
                 "swing; the old radius-ring lived here.")]
        [SerializeField] private GameObject visualPrefab;

        /// <summary>Same radius <see cref="Execute"/> uses for OverlapCircle (AttackData range, or fallback).</summary>
        public float GetEffectiveRadius(float attackRangeFromAttackData) =>
            attackRangeFromAttackData > 0f ? attackRangeFromAttackData : fallbackRadius;

        public override bool HasTargetInRange(in AttackContext ctx) =>
            TargetWithinRange(ctx, GetEffectiveRadius(ctx.range));

        /// <summary>
        /// Directional melee swing: damages enemies within the radius that fall inside a cone aimed at
        /// the nearest target, and spawns a transient slash visual pointed the same way. Falls back to a
        /// full-circle hit only if there is no target direction to swing toward.
        /// </summary>
        public override bool Execute(in AttackContext ctx)
        {
            if (ctx.attacker == null)
                return true;

            float r = GetEffectiveRadius(ctx.range);
            Vector2 origin = ctx.attacker.position;

            // Swing direction = toward the nearest target. If we have no direction, hit all around.
            Vector2 swingDir = Vector2.right;
            bool directed = false;
            if (ctx.target != null)
            {
                Vector2 toTarget = (Vector2)ctx.target.position - origin;
                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    swingDir = toTarget.normalized;
                    directed = arcAngleDegrees < 360f;
                }
            }

            var attackerRoot = ctx.attacker.root;
            var hits = Physics2D.OverlapCircleAll(origin, r, hitMask);
            var damaged = new HashSet<GameObject>();

            foreach (Collider2D hit in hits)
            {
                if (hit.transform.root == attackerRoot)
                    continue;

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable is not MonoBehaviour mb)
                    continue;

                if (!damaged.Add(mb.gameObject))
                    continue;

                // Only strike what's inside the swing arc.
                if (directed)
                {
                    Vector2 toHit = (Vector2)mb.transform.position - origin;
                    if (toHit.sqrMagnitude > 0.0001f &&
                        Vector2.Angle(swingDir, toHit) > arcAngleDegrees * 0.5f)
                        continue;
                }

                damageable.TakeDamage(ctx.damage);
            }

            SpawnSwingVisual(ctx.attacker, origin, swingDir);
            return true;
        }

        private void SpawnSwingVisual(Transform attacker, Vector2 origin, Vector2 swingDir)
        {
            if (swingVisualPrefab == null)
                return;

            float angle = Mathf.Atan2(swingDir.y, swingDir.x) * Mathf.Rad2Deg;
            Vector3 spawnPosition = (Vector3)(origin + swingDir * visualSpawnOffset);
            GameObject instance = Instantiate(
                swingVisualPrefab,
                spawnPosition,
                Quaternion.Euler(0f, 0f, angle + visualAngleOffset));

            SwingVisualRuntime runtime = instance.GetComponent<SwingVisualRuntime>();
            if (runtime == null)
                runtime = instance.AddComponent<SwingVisualRuntime>();

            runtime.Initialize(attacker, swingDir, visualLifetime, visualAngleOffset);
        }

        public override GameObject CreateVisual(Transform attacker) => visualPrefab;
    }
}
