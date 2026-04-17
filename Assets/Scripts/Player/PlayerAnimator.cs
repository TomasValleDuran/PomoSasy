using Input;
using UnityEngine;

namespace Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private InputHandler inputHandler;

        private Animator _animator;
        private Vector2 _lastDirection = Vector2.down;
        private static readonly int VelocityX = Animator.StringToHash("VelocityX");
        private static readonly int VelocityY = Animator.StringToHash("VelocityY");
        private static readonly int Speed = Animator.StringToHash("Speed");

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
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
    }
}
