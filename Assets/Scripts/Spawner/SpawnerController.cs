using UnityEngine;

namespace Spawner
{
    public class SpawnerController : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject entityPrefab;

        [Header("Spawn Settings")]
        public float spawnInterval = 10f;
        public Transform[] spawnPoints;

        private float _timer;

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
            if (spawnPoints.Length == 0) return;

            var point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Instantiate(entityPrefab, point.position, point.rotation);
        }
    }
}