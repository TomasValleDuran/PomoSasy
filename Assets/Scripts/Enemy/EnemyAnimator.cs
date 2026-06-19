using System.Collections;
using Controllers;
using UnityEngine;

namespace Enemy
{
    public class EnemyAnimator : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float attackAnimationDuration = 0.3f;

        private Animator _animator;
        private EnemyController _controller;
        private Coroutine _attackRoutine;
        private bool _hasAttackParam;
        private bool _hasDeadParam;
        private bool _hasDamagedParam;
        private float _lastHealth;

        private static readonly int IsWalking = Animator.StringToHash("isWalking");
        private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
        private static readonly int IsDead = Animator.StringToHash("isDead");
        private static readonly int IsDamaged = Animator.StringToHash("isDamaged");

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _controller = GetComponent<EnemyController>();
            _hasAttackParam = HasParameter(_animator, IsAttacking);
            _hasDeadParam = HasParameter(_animator, IsDead);
            _hasDamagedParam = HasParameter(_animator, IsDamaged);
            _controller.healthComponent.OnDamaged += OnHealthChanged;
        }

        private void OnEnable()
        {
            if (_controller != null)
            {
                _controller.OnMovingChanged += HandleMovingChanged;
                _controller.OnAttackPerformed += HandleAttackPerformed;
                _controller.OnDeathStarted += HandleDeathStarted;
            }

            // The enemy may be reused from the pool — clear any leftover attack/death state.
            _attackRoutine = null;
            if (_animator != null && _hasAttackParam)
                _animator.SetBool(IsAttacking, false);
            if (_animator != null && _hasDeadParam)
                _animator.SetBool(IsDead, false);
        }

        private void OnDisable()
        {
            if (_controller != null)
            {
                _controller.OnMovingChanged -= HandleMovingChanged;
                _controller.OnAttackPerformed -= HandleAttackPerformed;
                _controller.OnDeathStarted -= HandleDeathStarted;
            }
        }

        private void HandleDeathStarted()
        {
            if (_animator == null)
                return;

            // Stop any in-flight attack so it doesn't fight the death state.
            if (_attackRoutine != null)
            {
                StopCoroutine(_attackRoutine);
                _attackRoutine = null;
            }

            if (_hasAttackParam)
                _animator.SetBool(IsAttacking, false);
            _animator.SetBool(IsWalking, false);

            if (_hasDeadParam)
                _animator.SetBool(IsDead, true);
        }

        private void HandleMovingChanged(bool isMoving)
        {
            if (_animator != null)
                _animator.SetBool(IsWalking, isMoving);
        }

        // HealthComponent.OnDamaged fires on every health change, including spawn/reset/heal.
        // Only play the hit reaction when HP actually dropped.
        private void OnHealthChanged(float currentHealth)
        {
            if (currentHealth < _lastHealth && _animator != null && _hasDamagedParam)
                _animator.SetTrigger(IsDamaged);

            _lastHealth = currentHealth;
        }
        private void HandleAttackPerformed()
                {
                    if (_animator == null || !_hasAttackParam)
                        return;

                    if (_attackRoutine != null)
                        StopCoroutine(_attackRoutine);

                    _attackRoutine = StartCoroutine(PlayAttack());
                }

                private IEnumerator PlayAttack()
                {
                    _animator.SetBool(IsAttacking, true);
                    yield return new WaitForSeconds(attackAnimationDuration);
                    _animator.SetBool(IsAttacking, false);
                    _attackRoutine = null;
                }

                // Lets the attack animation be wired up later without spamming "parameter does not exist" warnings.
                private static bool HasParameter(Animator animator, int hash)
                {
                    if (animator == null)
                        return false;

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
