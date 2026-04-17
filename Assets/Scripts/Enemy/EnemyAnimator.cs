using Controllers;
using UnityEngine;

namespace Enemy
{
    public class EnemyAnimator : MonoBehaviour
    {
        private Animator _animator;
        private static readonly int IsWalking = Animator.StringToHash("isWalking");

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();

            EnemyController controller = GetComponent<EnemyController>();
            controller.OnMovingChanged += OnMovingChanged;
        }

        private void OnMovingChanged(bool isMoving)
        {
            _animator.SetBool(IsWalking, isMoving);
        }
    }
}
