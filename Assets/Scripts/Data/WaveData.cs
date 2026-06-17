using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class WaveDefinition
    {
        [Tooltip("Optional label shown in the UI (e.g. \"Skeletons\"). Falls back to \"Wave N\".")]
        public string waveName;

        [Tooltip("Which enemies (and how many of each) this wave spawns.")]
        public List<SpawnEntry> spawnEntries = new();

        [Tooltip("Seconds between each enemy spawn within this wave.")]
        public float spawnInterval = 1f;

        [Tooltip("Breather before this wave starts spawning (seconds).")]
        public float startDelay = 3f;

        public int TotalEnemies
        {
            get
            {
                int total = 0;
                if (spawnEntries == null) return 0;
                foreach (var entry in spawnEntries)
                {
                    if (entry != null && entry.prefab != null && entry.amount > 0)
                        total += entry.amount;
                }
                return total;
            }
        }
    }

    [CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
    public class WaveData : ScriptableObject
    {
        [Tooltip("Ordered list of waves. Difficulty rises by adding more / tougher enemies per wave.")]
        public List<WaveDefinition> waves = new();
    }
}
