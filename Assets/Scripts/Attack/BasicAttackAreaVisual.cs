using UnityEngine;

namespace Attack
{
    public class BasicAttackAreaVisual : MonoBehaviour, IAttackVisual
    {
        [SerializeField] private Transform radiusVisual;
        [SerializeField] private float diameterScaleMultiplier = 1f;

        private SpriteRenderer _spriteRenderer;
        private float _spriteDiameterAtScaleOne = 1f;

        private void Awake()
        {
            if (radiusVisual == null)
                radiusVisual = transform;

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

        public void Initialize(AttackData attackData, AttackBehavior behavior)
        {
            if (radiusVisual == null)
                radiusVisual = transform;

            if (_spriteRenderer == null)
            {
                _spriteRenderer = radiusVisual.GetComponent<SpriteRenderer>();
                CacheSpriteDiameter();
            }

            float radius = attackData != null ? attackData.AttackRange : 0f;
            if (behavior is BasicAttackBehavior basic)
                radius = basic.GetEffectiveRadius(radius);

            bool show = radius > 0f;
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
    }
}
