using UnityEngine;
using Data;
using System.Collections.Generic;
using UnityEditor;

namespace Spawner
{
    public class SpawnerController : MonoBehaviour
    {
        [Header("Data")]
        public SpawnerData spawnerData;

        [Header("Spawn Settings")]
        public float spawnRadius = 10f;

        private float _timer;
        private readonly List<GameObject> _spawnPool = new();

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
            Instantiate(entity, spawnPos, Quaternion.identity); // TODO: Use object pooling instead of instantiating
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
    }
}
