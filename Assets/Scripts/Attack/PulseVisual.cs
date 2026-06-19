using UnityEngine;

namespace Attack
{
    /// <summary>
    /// Gently "breathes" a sprite by oscillating its scale and alpha with a sine wave, so a static
    /// ring (e.g. the aura) feels alive. Multiplies on top of whatever base scale and color the object
    /// already has, so it cooperates with sizing logic like <see cref="BasicAttackAreaVisual"/> — the
    /// base size/tint is captured on the first frame, after that sizing has run.
    ///
    /// It also implements <see cref="IAttackPulse"/>: when the owning attack actually lands a hit,
    /// <see cref="Pulse"/> kicks a quick flare (extra scale + brightness) that decays, so the ring
    /// visibly reacts in sync with the damage on top of the idle breathing.
    ///
    /// Reusable: drop it on any SpriteRenderer object you want to pulse.
    /// </summary>
    public class PulseVisual : MonoBehaviour, IAttackPulse
    {
        [Tooltip("Renderer to fade. Leave empty to use the one on this object (or its children).")]
        [SerializeField] private SpriteRenderer targetRenderer;

        [Header("Idle breathing")]
        [Tooltip("Breaths per second. 0.8 => one full in-out about every 1.25s.")]
        [Min(0f)]
        [SerializeField] private float frequency = 0.8f;
        [Tooltip("How much the scale swings, as a fraction of the base scale. 0.05 => +/-5%.")]
        [Range(0f, 1f)]
        [SerializeField] private float scaleAmplitude = 0.06f;
        [Tooltip("How much the alpha swings, as a fraction of the base alpha. 0.6 => +/-60% of the base alpha.")]
        [Range(0f, 1f)]
        [SerializeField] private float alphaAmplitude = 0.6f;
        [Tooltip("Random phase so multiple pulsing objects don't beat in sync.")]
        [SerializeField] private bool randomizePhase = true;

        [Header("Flare on hit")]
        [Tooltip("Kick a flare each time the attack actually deals damage (driven by AttackSlot).")]
        [SerializeField] private bool flareOnAttack = true;
        [Tooltip("Extra scale at the peak of a flare, as a fraction of base scale. 0.25 => +25%.")]
        [Range(0f, 2f)]
        [SerializeField] private float flareScale = 0.25f;
        [Tooltip("Extra alpha at the peak of a flare, as a multiple of base alpha. 1.5 => +150%.")]
        [Range(0f, 5f)]
        [SerializeField] private float flareAlpha = 1.5f;
        [Tooltip("Seconds for a flare to fade back to the idle breathing.")]
        [Min(0.01f)]
        [SerializeField] private float flareDecay = 0.25f;

        private bool _captured;
        private Vector3 _baseScale = Vector3.one;
        private float _baseAlpha = 1f;
        private float _phase;
        private float _flare; // 1 at the instant of a hit, decays to 0

        private void OnEnable()
        {
            // Re-capture on re-enable; sizing/tint may have changed while disabled.
            _captured = false;
        }

        /// <summary>Called by <see cref="AttackSlot"/> when the owning attack lands a hit.</summary>
        public void Pulse()
        {
            if (flareOnAttack)
                _flare = 1f;
        }

        private void Capture()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponentInChildren<SpriteRenderer>();

            _baseScale = transform.localScale;
            _baseAlpha = targetRenderer != null ? targetRenderer.color.a : 1f;
            _phase = randomizePhase ? Random.value * Mathf.PI * 2f : 0f;
            _captured = true;
        }

        private void Update()
        {
            // Capture lazily on the first frame so any equip-time sizing has already been applied.
            if (!_captured)
                Capture();

            float wave = Mathf.Sin(Time.time * (Mathf.PI * 2f) * frequency + _phase); // -1..1

            if (_flare > 0f)
                _flare = Mathf.Max(0f, _flare - Time.deltaTime / flareDecay);

            float scaleMul = 1f + scaleAmplitude * wave + flareScale * _flare;
            transform.localScale = _baseScale * scaleMul;

            if (targetRenderer != null)
            {
                Color c = targetRenderer.color;
                c.a = Mathf.Max(0f, _baseAlpha * (1f + alphaAmplitude * wave + flareAlpha * _flare));
                targetRenderer.color = c;
            }
        }
    }
}
