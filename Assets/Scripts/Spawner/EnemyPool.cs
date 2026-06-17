using System.Collections.Generic;
using UnityEngine;

namespace Spawner
{
    /// <summary>
    /// Reuses inactive enemy instances per prefab instead of instantiating new ones.
    /// Same pooling approach as <see cref="SpawnerController"/>, extracted so the wave
    /// system can share it.
    /// </summary>
    public class EnemyPool
    {
        private readonly Dictionary<GameObject, List<GameObject>> _instancesByPrefab = new();

        public GameObject Get(GameObject prefab, Vector3 position)
        {
            if (TryGetInactiveInstance(prefab, out GameObject pooled))
            {
                pooled.transform.SetPositionAndRotation(position, Quaternion.identity);
                pooled.SetActive(true);
                return pooled;
            }

            GameObject spawned = Object.Instantiate(prefab, position, Quaternion.identity);
            RegisterInstance(prefab, spawned);
            return spawned;
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
