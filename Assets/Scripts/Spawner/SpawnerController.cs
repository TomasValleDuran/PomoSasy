using UnityEngine;
using Data;
using System.Collections.Generic;

namespace Spawner
{
    public class SpawnerController : MonoBehaviour
    {
        [Header("Data")]
        public SpawnerData spawnerData;

        [Header("Spawn Settings")]
        public float spawnRadius = 10f;
        [SerializeField] private bool useEnemyPooling = true;

        private float _timer;
        private readonly List<GameObject> _spawnPool = new();
        private readonly Dictionary<GameObject, List<GameObject>> _instancesByPrefab = new();

        void Update()
        {
            if (spawnerData == null || GameManagerScript.Instance.Player == null) return;

            if (spawnerData.spawnInterval <= 0f)
            {
                SpawnNextEntity();
                return;
            }

            _timer += Time.deltaTime;

            if (_timer >= spawnerData.spawnInterval)
            {
                SpawnNextEntity();
                _timer -= spawnerData.spawnInterval;
            }
        }

        void SpawnNextEntity()
        {
            if (_spawnPool.Count == 0)
            {
                RebuildSpawnPool();
            }

            if (_spawnPool.Count == 0)
            {
                return;
            }

            GameObject prefab = _spawnPool[^1];
            _spawnPool.RemoveAt(_spawnPool.Count - 1);

            Vector3 spawnPos = GetRandomPositionAroundPlayer();
            Spawn(prefab, spawnPos);
        }

        private void Spawn(GameObject entity, Vector3 spawnPos)
        {
            if (!useEnemyPooling)
            {
                Instantiate(entity, spawnPos, Quaternion.identity);
                return;
            }

            if (TryGetInactiveInstance(entity, out GameObject pooledEnemy))
            {
                pooledEnemy.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
                pooledEnemy.SetActive(true);
                return;
            }

            GameObject spawnedEnemy = Instantiate(entity, spawnPos, Quaternion.identity);
            RegisterInstance(entity, spawnedEnemy);
        }

        void RebuildSpawnPool()
        {
            _spawnPool.Clear();

            if (spawnerData.spawnEntries == null)
            {
                return;
            }

            foreach (var entry in spawnerData.spawnEntries)
            {
                if (entry.prefab == null || entry.amount <= 0) continue;

                for (int i = 0; i < entry.amount; i++)
                {
                    _spawnPool.Add(entry.prefab);
                }
            }

            for (int i = 0; i < _spawnPool.Count; i++)
            {
                int rand = Random.Range(i, _spawnPool.Count);
                (_spawnPool[i], _spawnPool[rand]) = (_spawnPool[rand], _spawnPool[i]);
            }
        }

        Vector3 GetRandomPositionAroundPlayer()
        {
            Vector3 playerPos = GameManagerScript.Instance.Player.position;
            Vector2 circle = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 offset = new Vector3(circle.x, circle.y, 0f);

            return playerPos + offset;
        }

        private bool TryGetInactiveInstance(GameObject prefab, out GameObject instance)
        {
            instance = null;

            if (!_instancesByPrefab.TryGetValue(prefab, out List<GameObject> instances))
                return false;

            for (int i = 0; i < instances.Count; i++)
            {
                GameObject candidate = instances[i];
                if (candidate == null)
                    continue;

                if (!candidate.activeInHierarchy)
                {
                    instance = candidate;
                    return true;
                }
            }

            return false;
        }

        private void RegisterInstance(GameObject prefab, GameObject instance)
        {
            if (!_instancesByPrefab.TryGetValue(prefab, out List<GameObject> instances))
            {
                instances = new List<GameObject>();
                _instancesByPrefab[prefab] = instances;
            }

            instances.Add(instance);
        }
    }
}
