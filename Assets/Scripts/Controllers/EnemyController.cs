using System;
using System.Collections.Generic;
using System.Linq;
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

        public event Action<bool> OnMovingChanged;

        private enum State { Chase, WindUp, Attack, Cooldown }

        private State _currentState = State.Chase;
        private bool _isMoving;
        private bool _isRegistered;

        private float _stateTimer;

        private void Awake()
        {
            GetComponent<HealthComponent>().OnDeath += HandleDeath;
            if (GameManagerScript.Instance != null)
            {
                playerTransform = GameManagerScript.Instance.Player;
            }
        }

        private void OnEnable()
        {
            if (playerTransform == null && GameManagerScript.Instance != null)
            {
                playerTransform = GameManagerScript.Instance.Player;
            }

            _currentState = State.Chase;
            _stateTimer = 0f;
            SetMoving(false);
            RegisterEnemy();
        }

        private void OnDisable()
        {
            UnregisterEnemy();
        }

        private void Update()
        {
            if (data == null || data.AttackData == null)
                return;

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
            if (playerTransform == null)
            {
                SetMoving(false);
                return;
            }

            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (distance <= data.AttackData.AttackRange)
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
            _stateTimer = data.AttackData.WindupDuration;
        }

        private void UpdateWindUp()
        {
            _stateTimer -= Time.deltaTime;

            if (_stateTimer <= 0f)
                _currentState = State.Attack;
        }

        private void UpdateAttack()
        {
            bool finished = data.AttackData.AttackBehavior.Execute(transform, playerTransform, data.AttackData.Damage, data.AttackData.AttackRange);

            if (finished)
                EnterCooldown();
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
            if (deactivateOnDeath)
            {
                gameObject.SetActive(false);
                return;
            }

            Destroy(gameObject);
        }
        
        private void DropLoot()
        {
            var points = data.PointsOnDeath;
            var coinsLoot = new List<PomoSasyConstants.CoinType>();

            var coinMap = new Dictionary<PomoSasyConstants.CoinType, int>
            {
                { PomoSasyConstants.CoinType.Copper, PomoSasyConstants.CoinValues.Copper },
                { PomoSasyConstants.CoinType.Silver, PomoSasyConstants.CoinValues.Silver },
                { PomoSasyConstants.CoinType.Gold, PomoSasyConstants.CoinValues.Gold },
                { PomoSasyConstants.CoinType.Platinum, PomoSasyConstants.CoinValues.Platinum }
            };

            var rng = new Random();

            // --- 1. Split into chunks ---
            var chunks = new List<int>();
            int remaining = points;

            while (remaining > 0)
            {
                // Chunk size: between 10% and 40% of remaining (tweakable)
                int minChunk = Math.Max(1, remaining / 10);
                int maxChunk = Math.Max(1, remaining / 2);

                int chunk = rng.Next(minChunk, maxChunk + 1);
                chunk = Math.Min(chunk, remaining);

                chunks.Add(chunk);
                remaining -= chunk;
            }

            // --- 2. Generate coins per chunk ---
            foreach (var chunk in chunks)
            {
                int chunkRemaining = chunk;

                while (chunkRemaining > 0)
                {
                    var validCoins = coinMap
                        .Where(kv => kv.Value <= chunkRemaining)
                        .ToList();

                    // Slight bias toward bigger coins, but not absolute
                    var chosen = validCoins
                        .OrderByDescending(kv => kv.Value + rng.Next(-3, 4)) // controlled randomness
                        .First();

                    coinsLoot.Add(chosen.Key);
                    chunkRemaining -= chosen.Value;
                }
            }
        }
    }
}
