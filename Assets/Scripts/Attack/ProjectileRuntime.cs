using System.Collections;
using Health;
using UnityEngine;

namespace Attack
{
    public class ProjectileRuntime : MonoBehaviour
    {
        private static readonly int OnHitHash = Animator.StringToHash("OnHit");

        [SerializeField] private float hitAnimationDuration = 0.2f;
        [SerializeField] private float visualAngleOffset = 0f;

        private Transform _owner;
        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private float _remainingDistance;
        private LayerMask _hitMask;
        private AudioClip _attackSfx;
        private bool _initialized;
        private bool _isDespawning;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        public void Initialize(Transform owner, Vector2 direction, float speed, float damage, float maxDistance, LayerMask hitMask, AudioClip attackSfx = null)
        {
            _owner = owner;
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + visualAngleOffset);
            _speed = Mathf.Max(speed, 0.01f);
            _damage = damage;
            _remainingDistance = Mathf.Max(maxDistance, 0.01f);
            _hitMask = hitMask;
            _attackSfx = attackSfx;
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized || _isDespawning)
                return;

            float step = _speed * Time.deltaTime;
            if (step <= 0f)
                return;

            float castDistance = Mathf.Min(step, _remainingDistance);
            Vector2 start = transform.position;

            // Look at everything along this step, not just the closest collider, so the projectile
            // flies *through* non-combat things (coins, walls, props) and the owner, and only
            // reacts to the first thing that can actually take damage.
            RaycastHit2D[] hits = Physics2D.RaycastAll(start, _direction, castDistance, _hitMask);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D collider = hits[i].collider;
                if (collider == null)
                    continue;

                // Ignore the shooter itself.
                if (_owner != null && hits[i].transform.root == _owner.root)
                    continue;

                // No health = not a target (coins, scenery). Pass through it.
                IDamageable damageable = collider.GetComponentInParent<IDamageable>();
                if (damageable == null)
                    continue;
                AttackAudioPlayer.PlayAtPoint(hit.point, _attackSfx);
                damageable.TakeDamage(_damage);
                BeginDespawn();
                return;
            }

            transform.position = (Vector3)(start + (_direction * castDistance));
            _remainingDistance -= castDistance;

            if (_remainingDistance <= 0f)
                BeginDespawn();
        }

        private void BeginDespawn()
        {
            if (_isDespawning)
                return;

            _isDespawning = true;

            if (_animator == null)
            {
                Destroy(gameObject);
                return;
            }

            _animator.SetTrigger(OnHitHash);

            if (hitAnimationDuration <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            StartCoroutine(DestroyAfterHitAnimation());
        }

        private IEnumerator DestroyAfterHitAnimation()
        {
            yield return new WaitForSeconds(hitAnimationDuration);
            Destroy(gameObject);
        }
    }
}
