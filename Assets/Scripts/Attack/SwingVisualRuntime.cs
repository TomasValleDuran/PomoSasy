using UnityEngine;

namespace Attack
{
    /// <summary>
    /// Short-lived visual for a directional melee swing. Spawned per attack by
    /// <see cref="BasicAttackBehavior"/>, oriented toward the target, parented to the attacker so it
    /// follows during its brief life, then self-destructs.
    ///
    /// By default it animates itself: a quick scale-up while fading out, so the swing "pops" instead
    /// of blinking on and off. Turn <c>animateProcedurally</c> off if you drive the look with your own
    /// Animator clip instead; in that case an "OnAttack" trigger (if present) is fired on spawn.
    /// </summary>
    public class SwingVisualRuntime : MonoBehaviour
    {
        private static readonly int OnAttackHash = Animator.StringToHash("OnAttack");

        [Tooltip("Keep the swing attached to the attacker for its lifetime so it tracks the player.")]
        [SerializeField] private bool followAttacker = true;

        [Header("Procedural pop (scale-up + fade-out)")]
        [Tooltip("Animate scale and alpha in code over the visual's lifetime. Turn off to use your own Animator clip.")]
        [SerializeField] private bool animateProcedurally = true;
        [Tooltip("Scale multiplier at spawn, relative to the prefab's authored scale.")]
        [SerializeField] [Min(0f)] private float startScale = 0.65f;
        [Tooltip("Scale multiplier at the end of the swing. Above 1 gives a slight overshoot.")]
        [SerializeField] [Min(0f)] private float endScale = 1.1f;
        [Tooltip("Fraction of the lifetime the swing stays fully opaque before it starts fading (0..1).")]
        [Range(0f, 1f)]
        [SerializeField] private float fadeHoldFraction = 0.25f;

        private float _elapsed;
        private float _lifetime;
        private bool _animating;
        private Vector3 _baseScale = Vector3.one;
        private SpriteRenderer[] _renderers;
        private float[] _baseAlphas;

        /// <param name="attacker">Player the swing belongs to; the visual follows it when <c>followAttacker</c> is set.</param>
        /// <param name="direction">World-space direction the swing points toward.</param>
        /// <param name="lifetime">Seconds before the visual destroys itself. &lt;= 0 destroys immediately.</param>
        /// <param name="angleOffset">Added to the computed facing angle to align art whose "forward" isn't +X.</param>
        public void Initialize(Transform attacker, Vector2 direction, float lifetime, float angleOffset = 0f)
        {
            Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + angleOffset);

            if (followAttacker && attacker != null)
                transform.SetParent(attacker, worldPositionStays: true);

            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null && HasParameter(animator, OnAttackHash))
                animator.SetTrigger(OnAttackHash);

            if (lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            _lifetime = lifetime;

            if (animateProcedurally)
            {
                _baseScale = transform.localScale;
                _renderers = GetComponentsInChildren<SpriteRenderer>(true);
                _baseAlphas = new float[_renderers.Length];
                for (int i = 0; i < _renderers.Length; i++)
                    _baseAlphas[i] = _renderers[i].color.a;

                ApplyFrame(0f);
                _animating = true;
            }
            else
            {
                // Animator (or nothing) drives the look; just clean up at the end.
                Destroy(gameObject, lifetime);
            }
        }

        private void Update()
        {
            if (!_animating)
                return;

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _lifetime);
            ApplyFrame(t);

            if (t >= 1f)
                Destroy(gameObject);
        }

        private void ApplyFrame(float t)
        {
            // Scale: ease-out so it snaps open quickly then settles.
            float scaleEase = 1f - (1f - t) * (1f - t);
            transform.localScale = _baseScale * Mathf.LerpUnclamped(startScale, endScale, scaleEase);

            // Alpha: hold, then fade to zero over the remainder of the lifetime.
            float fade = fadeHoldFraction >= 1f
                ? 1f
                : Mathf.Clamp01((t - fadeHoldFraction) / (1f - fadeHoldFraction));
            float alphaMul = 1f - fade * fade;

            if (_renderers == null)
                return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                SpriteRenderer sr = _renderers[i];
                if (sr == null)
                    continue;

                Color c = sr.color;
                c.a = _baseAlphas[i] * alphaMul;
                sr.color = c;
            }
        }

        // Avoids "parameter does not exist" warnings when the prefab has no Animator wired up yet.
        private static bool HasParameter(Animator animator, int hash)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].nameHash == hash)
                    return true;
            }

            return false;
        }
    }
}
