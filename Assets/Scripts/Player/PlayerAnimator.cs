using System.Collections;
using Attack;
using Input;
using UnityEngine;

namespace Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private InputHandler inputHandler;
        [SerializeField] private PlayerAttackLoadout loadout;
        [SerializeField, Min(0f)] private float attackAnimationDuration = 0.2f;

        private Animator _animator;
        private Vector2 _lastDirection = Vector2.down;
        private Coroutine _attackRoutine;
        private bool _hasAttackParam;
        private static readonly int VelocityX = Animator.StringToHash("VelocityX");
        private static readonly int VelocityY = Animator.StringToHash("VelocityY");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsAttacking = Animator.StringToHash("isAttacking");

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            if (loadout == null)
                loadout = GetComponentInParent<PlayerAttackLoadout>();
            _hasAttackParam = HasParameter(_animator, IsAttacking);
        }

        private void OnEnable()
        {
            if (loadout != null)
                loadout.OnAttackPerformed += HandleAttackPerformed;
        }

        private void OnDisable()
        {
            if (loadout != null)
                loadout.OnAttackPerformed -= HandleAttackPerformed;
        }

        private void Update()
        {
            Vector2 input = inputHandler.MoveInput;

            if (input != Vector2.zero)
                _lastDirection = input.normalized;

            _animator.SetFloat(VelocityX, _lastDirection.x);
            _animator.SetFloat(VelocityY, _lastDirection.y);
            _animator.SetFloat(Speed, input.magnitude);
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
