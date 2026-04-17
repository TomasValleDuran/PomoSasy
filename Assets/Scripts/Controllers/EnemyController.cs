using System;
using Data;
using UnityEngine;

namespace Controllers
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyData data;
        [SerializeField] private Transform playerTransform;

        public event Action<bool> OnMovingChanged;

        private enum State { Chase, WindUp, Attack, Cooldown }

        private State _currentState = State.Chase;
        private bool _isMoving;
        private bool _isRegistered;

        private float _stateTimer;
        private Vector2 _dashTarget;

        private void Awake()
        {
            playerTransform = GameManagerScript.Instance.Player;
        }

        private void OnEnable()
        {
            RegisterEnemy();
        }

        private void OnDisable()
        {
            UnregisterEnemy();
        }

        private void Update()
        {
            switch (_currentState)
            {
                case State.Chase:    UpdateChase();    break;
                case State.WindUp:   UpdateWindUp();   break;
                case State.Attack:   UpdateAttack();   break;
                case State.Cooldown: UpdateCooldown(); break;
            }
        }

        private void UpdateChase()
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (distance <= data.AttackRange)
            {
                SetMoving(false);
                EnterWindUp();
                return;
            }

            SetMoving(true);
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            transform.position += (Vector3)direction * (data.MoveSpeed * Time.deltaTime);
        }

        private void SetMoving(bool moving)
        {
            if (_isMoving == moving) return;
            _isMoving = moving;
            OnMovingChanged?.Invoke(_isMoving);
        }

        private void EnterWindUp()
        {
            SetMoving(false);
            _currentState = State.WindUp;
            _stateTimer = data.WindupDuration;
            _dashTarget = playerTransform.position;
        }

        private void UpdateWindUp()
        {
            _stateTimer -= Time.deltaTime;

            if (_stateTimer <= 0f)
                _currentState = State.Attack;
        }

        private void UpdateAttack()
        {
            bool finished = data.AttackBehavior.Execute(transform, _dashTarget, data.AttackDamage);

            if (finished)
                EnterCooldown();
        }

        private void EnterCooldown()
        {
            _currentState = State.Cooldown;
            _stateTimer = data.AttackCooldown;
        }

        private void UpdateCooldown()
        {
            _stateTimer -= Time.deltaTime;

            if (_stateTimer <= 0f)
                _currentState = State.Chase;
        }

        private void RegisterEnemy()
        {
            if (_isRegistered || GameManagerScript.Instance == null || data == null)
            {
                return;
            }

            GameManagerScript.Instance.RegisterEnemy(data.EnemyType);
            _isRegistered = true;
        }

        private void UnregisterEnemy()
        {
            if (!_isRegistered || GameManagerScript.Instance == null || data == null)
            {
                return;
            }

            GameManagerScript.Instance.UnregisterEnemy(data.EnemyType);
            _isRegistered = false;
        }
    }
}
