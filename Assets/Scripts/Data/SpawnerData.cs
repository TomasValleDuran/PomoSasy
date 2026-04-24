using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        public int amount;
    }

    [CreateAssetMenu(fileName = "SpawnerData", menuName = "Scriptable Objects/SpawnerData")]
    public class SpawnerData : ScriptableObject
    {
        public float spawnInterval;
        public List<SpawnEntry> spawnEntries;
    }
}