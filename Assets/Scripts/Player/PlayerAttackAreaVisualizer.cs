using Attack;
using Data;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// Scales a child SpriteRenderer so its diameter matches the attack radius (same space as Physics2D overlap).
    /// Uses the sprite mesh size so Pixels Per Unit / texture size do not need manual guessing.
    /// </summary>
    public class PlayerAttackAreaVisualizer : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [Tooltip("Child with SpriteRenderer (circle). Scale is driven from sprite bounds + attack radius.")]
        [SerializeField] private Transform radiusVisual;
        [Tooltip("Extra factor after auto-fit (1 = matches AttackData attack range / BasicAttack overlap).")]
        [SerializeField] private float diameterScaleMultiplier = 1f;
        [SerializeField] private bool hideWhenNotBasicAttack = true;

        [Header("Debug (Scene view)")]
        [SerializeField] private bool drawPhysicsRadiusGizmo;
        [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0.25f, 0.9f);

        private SpriteRenderer _spriteRenderer;
        private float _spriteDiameterAtScaleOne = 1f;

        private void Awake()
        {
            if (radiusVisual != null)
            {
                _spriteRenderer = radiusVisual.GetComponent<SpriteRenderer>();
                CacheSpriteDiameter();
            }
        }

        private void OnValidate()
        {
            if (radiusVisual != null && _spriteRenderer == null)
                _spriteRenderer = radiusVisual.GetComponent<SpriteRenderer>();
            CacheSpriteDiameter();
        }

        private void CacheSpriteDiameter()
        {
            if (_spriteRenderer == null || _spriteRenderer.sprite == null)
            {
                _spriteDiameterAtScaleOne = 1f;
                return;
            }

            Vector3 size = _spriteRenderer.sprite.bounds.size;
            _spriteDiameterAtScaleOne = Mathf.Max(size.x, size.y);
            if (_spriteDiameterAtScaleOne < 0.0001f)
                _spriteDiameterAtScaleOne = 1f;
        }

        private void LateUpdate()
        {
            if (radiusVisual == null || playerData?.AttackData == null)
                return;

            float radius = ResolveRadius();
            bool show = radius > 0f;

            if (hideWhenNotBasicAttack && playerData.AttackData.AttackBehavior is not BasicAttackBehavior)
                show = false;

            if (_spriteRenderer != null)
                _spriteRenderer.enabled = show;

            if (!show)
                return;

            float targetDiameter = radius * 2f * diameterScaleMultiplier;
            float parentUniform = 1f;
            if (radiusVisual.parent != null)
            {
                Vector3 p = radiusVisual.parent.lossyScale;
                parentUniform = Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y));
            }

            float uniform = targetDiameter / (_spriteDiameterAtScaleOne * parentUniform);
            radiusVisual.localScale = new Vector3(uniform, uniform, 1f);
        }

        private float ResolveRadius()
        {
            if (playerData?.AttackData == null)
                return 0f;

            AttackData ad = playerData.AttackData;
            if (ad.AttackBehavior is BasicAttackBehavior basic)
                return basic.GetEffectiveRadius(ad.AttackRange);

            return ad.AttackRange;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawPhysicsRadiusGizmo || playerData?.AttackData == null)
                return;

            float r = ResolveRadius();
            if (r <= 0f)
                return;

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, r);
        }
    }
}
