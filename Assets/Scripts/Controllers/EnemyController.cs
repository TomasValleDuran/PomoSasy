using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Attack;
using Data;
using Health;
using UnityEngine;
using Random = System.Random;

namespace Controllers
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyData data;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private bool deactivateOnDeath = true;
        [Tooltip("Seconds to keep the enemy alive after death so a Death animation can play. " +
                 "Leave at 0 for enemies with no death animation (they disappear instantly).")]
        [SerializeField, Min(0f)] private float deathAnimationDuration = 0f;
        [SerializeField] public AudioSource attackAudioSource;

        public event Action<bool> OnMovingChanged;
        /// <summary>Raised once at the moment a strike begins (after wind-up). Drives the attack animation.</summary>
        public event Action OnAttackPerformed;
        /// <summary>Raised the moment HP hits 0, before the enemy is removed. Drives the death animation.</summary>
        public event Action OnDeathStarted;

        private enum State { Chase, WindUp, Attack, Cooldown }

        private State _currentState = State.Chase;
        private bool _isMoving;
        private bool _isRegistered;
        private bool _isDying;

        private float _stateTimer;

        private void Awake()
        {
            GetComponent<HealthComponent>().OnDeath += HandleDeath;

            if (!TryGetComponent(out attackAudioSource))
                attackAudioSource = gameObject.AddComponent<AudioSource>();

            if (GameManagerScript.Instance != null)
                playerTransform = GameManagerScript.Instance.Player;
        }

        private void OnEnable()
        {
            if (playerTransform == null && GameManagerScript.Instance != null)
            {
                playerTransform = GameManagerScript.Instance.Player;
            }

            _currentState = State.Chase;
            _stateTimer = 0f;
            _isDying = false;
            SetMoving(false);
            RegisterEnemy();
        }

        private void OnDisable()
        {
            UnregisterEnemy();
        }

        private void Update()
        {
            if (_isDying || !data || data.AttackData == null)
                return;

            switch (_currentState)
            {
                case State.Chase:    UpdateChase();    break;
                case State.WindUp:   UpdateWindUp();   break;
                case State.Attack:   UpdateAttack();   break;
                case State.Cooldown: UpdateCooldown(); break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateChase()
        {
            if (!playerTransform)
            {
                // The player may not have existed when this enemy was enabled (e.g. an enemy
                // placed in the scene before the player spawns). Keep trying to acquire it.
                if (GameManagerScript.Instance != null)
                    playerTransform = GameManagerScript.Instance.Player;

                if (!playerTransform)
                {
                    SetMoving(false);
                    return;
                }
            }

            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (distance <= data.AttackData.AttackRange)
            {
                SetMoving(false);
                EnterWindUp();
                return;
            }

            Vector2 direction = ResolveMoveDirection();
            SetMoving(direction != Vector2.zero);
            transform.position += (Vector3)direction * (data.MoveSpeed * Time.deltaTime);
        }

        // Asks the enemy's MovementPolicy where to go; falls back to chasing the player straight.
        private Vector2 ResolveMoveDirection()
        {
            if (data.MovementPolicy != null)
                return data.MovementPolicy.GetDirection(transform, playerTransform);

            return ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
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
            _stateTimer = data.AttackData.WindupDuration;

            // Start the swing animation NOW. The actual hit lands when the wind-up ends, so
            // WindupDuration = "time from the swing starting until contact". Tune it to match
            // the contact frame of the Attack clip.
            OnAttackPerformed?.Invoke();
        }

        private void UpdateWindUp()
        {
            _stateTimer -= Time.deltaTime;

            if (_stateTimer <= 0f)
                _currentState = State.Attack;
        }

        private void UpdateAttack()
        {
            var ctx = new Attack.AttackContext(
                transform,
                playerTransform,
                data.AttackData.Damage,
                data.AttackData.AttackRange,
                Time.deltaTime
            );
            bool finished = data.AttackData.AttackBehavior.Execute(ctx);

            if (finished)
            {
                AttackAudioPlayer.Play(attackAudioSource, data.AttackData);
                EnterCooldown();
            }
        }

        private void EnterCooldown()
        {
            _currentState = State.Cooldown;
            _stateTimer = data.AttackData.Cooldown;
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

        private void HandleDeath()
        {
            if (_isDying)
                return;

            _isDying = true;
            SetMoving(false);
            OnDeathStarted?.Invoke();

            if (deathAnimationDuration > 0f)
                StartCoroutine(FinishDeathAfter(deathAnimationDuration));
            else
                FinishDeath();
        }

        private IEnumerator FinishDeathAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            FinishDeath();
        }

        private void FinishDeath()
        {
            // Drop loot/XP as the body is removed — after the death animation — so the
            // coins don't spawn on top of it and hide the death.
            DropLoot();
            GiveXp();

            if (deactivateOnDeath)
            {
                gameObject.SetActive(false);
                return;
            }

            Destroy(gameObject);
        }


        private void DropLoot()
        {
            LootManagerScript.Instance.DropCoins(data.MoneyOnDeath, transform.position);
        }

        private void GiveXp()
        {
            XpManagerScript.Instance.Add(data.XpOnDeath);
        }
    }
}
