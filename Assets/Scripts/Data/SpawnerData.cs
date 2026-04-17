using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    public class SpawnerData : ScriptableObject
    {
        public float spawnInterval;
        public GameObject entityPrefab;
    }
}