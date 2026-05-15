using Health;
using UnityEngine;

namespace Attack
{
    public class ProjectileRuntime : MonoBehaviour
    {
        private Transform _owner;
        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private float _remainingDistance;
        private LayerMask _hitMask;
        private bool _initialized;

        public void Initialize(Transform owner, Vector2 direction, float speed, float damage, float maxDistance, LayerMask hitMask)
        {
            _owner = owner;
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            _speed = Mathf.Max(speed, 0.01f);
            _damage = damage;
            _remainingDistance = Mathf.Max(maxDistance, 0.01f);
            _hitMask = hitMask;
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized)
                return;

            float step = _speed * Time.deltaTime;
            if (step <= 0f)
                return;

            float castDistance = Mathf.Min(step, _remainingDistance);
            Vector2 start = transform.position;
            RaycastHit2D hit = Physics2D.Raycast(start, _direction, castDistance, _hitMask);
            if (hit.collider != null && (_owner == null || hit.transform.root != _owner.root))
            {
                hit.collider.GetComponentInParent<IDamageable>()?.TakeDamage(_damage);
                Destroy(gameObject);
                return;
            }

            transform.position = (Vector3)(start + (_direction * castDistance));
            _remainingDistance -= castDistance;

            if (_remainingDistance <= 0f)
                Destroy(gameObject);
        }
    }
}
