using UnityEngine;
using System.Collections.Generic;

namespace Spawner
{
    public class SpawnerController : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject entityPrefab;

        [Header("Spawn Settings")]
        public float spawnInterval = 10f;
        public Transform[] spawnPoints;
        
        [Header("Pool Settings")]
        [SerializeField] private int initialPoolSize = 15;
        [SerializeField] private bool allowPoolExpansion = false;

        private float _timer;
        private readonly List<GameObject> _pool = new();

        private void Awake()
        {
            InitializePool();
        }

        void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= spawnInterval)
            {
                SpawnEnemy();
                _timer = 0f;
            }
        }

        void SpawnEnemy()
        {
            if (entityPrefab == null || spawnPoints.Length == 0) return;

            var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var enemy = GetPooledEnemy();
            if (enemy == null) return;

            enemy.transform.SetPositionAndRotation(point.position, point.rotation);
            enemy.SetActive(true);
        }

        private void InitializePool()
        {
            _pool.Clear();

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePooledEnemy();
            }
        }

        private GameObject GetPooledEnemy()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (!_pool[i].activeInHierarchy)
                {
                    return _pool[i];
                }
            }

            if (!allowPoolExpansion)
            {
                return null;
            }

            return CreatePooledEnemy();
        }

        private GameObject CreatePooledEnemy()
        {
            var enemy = Instantiate(entityPrefab, transform.position, Quaternion.identity, transform);
            enemy.SetActive(false);
            _pool.Add(enemy);
            return enemy;
        }
    }
}