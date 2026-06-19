using Health;
using UnityEngine;

namespace Attack
{
    [CreateAssetMenu(fileName = "DashAttack", menuName = "Scriptable Objects/Attacks/Dash")]
    public class DashAttackBehavior : AttackBehavior
    {
        [SerializeField] private float dashSpeed = 10f;
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] public AudioClip dashSfx;
        public float DashSpeed => dashSpeed;

        public override bool Execute(in AttackContext ctx)
        {
            return ExecuteWithResult(ctx).Finished;
        }

        public override AttackExecutionResult ExecuteWithResult(in AttackContext ctx)
        {
            if (ctx.attacker == null || ctx.target == null)
                return new AttackExecutionResult(true, false);

            DashAttackRuntime runtime = ctx.attacker.GetComponent<DashAttackRuntime>();
            if (runtime == null)
                runtime = ctx.attacker.gameObject.AddComponent<DashAttackRuntime>();

            if (!runtime.IsActive)
            {
                Vector2 direction = ((Vector2)ctx.target.position - (Vector2)ctx.attacker.position).normalized;
                if (direction.sqrMagnitude < 0.0001f)
                    direction = Vector2.right;

                float travelDistance = Mathf.Max(ctx.range, 0.1f);
                runtime.Begin(direction, travelDistance);
                AttackAudioPlayer.PlayAtPoint(ctx.attacker.position, dashSfx != null ? dashSfx : ctx.attackSfx);
            }

            float step = dashSpeed * ctx.deltaTime;
            float moveAmount = Mathf.Min(step, runtime.RemainingDistance);
            ctx.attacker.position += (Vector3)(runtime.Direction * moveAmount);
            runtime.RemainingDistance -= moveAmount;

            float contactDistance = ctx.range > 0f ? Mathf.Min(ctx.range, 0.35f) : 0.1f;
            bool hitThisFrame = false;
            if (!runtime.HasHit && Vector2.Distance(ctx.attacker.position, ctx.target.position) <= contactDistance)
            {
                IDamageable damageable = ctx.target.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(ctx.damage);
                    hitThisFrame = true;
                }

                runtime.HasHit = true;
            }

            bool finished = runtime.HasHit || runtime.RemainingDistance <= 0f;
            if (finished)
                runtime.ResetState();

            return new AttackExecutionResult(finished, hitThisFrame, false);
        }

        public override GameObject CreateVisual(Transform attacker) => visualPrefab;
    }
}
