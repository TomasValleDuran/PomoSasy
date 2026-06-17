using System;

namespace Spawner
{
    /// <summary>
    /// Plain DTOs matching the waves JSON file, parsed by Unity's <c>JsonUtility</c>.
    /// Prefabs can't live in JSON, so enemies are referenced by an <see cref="WaveEntryJson.enemy"/>
    /// id that the <see cref="WaveSpawner"/> resolves through its enemy catalog.
    /// </summary>
    [Serializable]
    public class WaveConfigJson
    {
        public WaveJson[] waves;
    }

    [Serializable]
    public class WaveJson
    {
        public string waveName;
        public float spawnInterval = 1f;
        public float startDelay = 3f;
        public WaveEntryJson[] entries;
    }

    [Serializable]
    public class WaveEntryJson
    {
        public string enemy;
        public int amount;
    }
}
