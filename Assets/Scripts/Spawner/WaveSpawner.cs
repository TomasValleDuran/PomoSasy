using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Save;
using UnityEngine;

namespace Spawner
{
    /// <summary>
    /// Spawns enemies in discrete waves. Each wave spawns its roster over time, then waits
    /// until every enemy is dead before the next wave begins. When the last wave is cleared
    /// the run is won (<see cref="OnAllWavesCompleted"/>).
    ///
    /// Waves come from a JSON <see cref="TextAsset"/> if one is assigned (enemies referenced by
    /// id and resolved through <see cref="enemyCatalog"/>); otherwise from the <see cref="waveData"/>
    /// ScriptableObject. On "Continue" it resumes at the start of the saved wave.
    ///
    /// Replaces <see cref="SpawnerController"/> in the scene (don't run both at once, or the
    /// "all enemies dead" check will never settle).
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        [Serializable]
        public class EnemyCatalogEntry
        {
            [Tooltip("The \"enemy\" id used in the JSON.")]
            public string id;
            public GameObject prefab;
        }

        [Header("Wave source (JSON takes priority over Wave Data)")]
        [Tooltip("JSON file describing the waves. If assigned, Wave Data is ignored. See Assets/Config/waves.json.")]
        [SerializeField] private TextAsset wavesJson;
        [Tooltip("Fallback waves used only when no JSON is assigned.")]
        public WaveData waveData;

        [Header("Enemy catalog — maps each JSON \"enemy\" id to a prefab")]
        [SerializeField] private List<EnemyCatalogEntry> enemyCatalog = new();

        [Header("Spawn Settings")]
        public float spawnRadius = 10f;
        [SerializeField] private bool useEnemyPooling = true;

        /// <summary>(currentWave 1-based, totalWaves)</summary>
        public event Action<int, int> OnWaveStarted;
        /// <summary>currentWave 1-based</summary>
        public event Action<int> OnWaveCompleted;
        public event Action OnAllWavesCompleted;

        public int CurrentWaveNumber { get; private set; }
        public int TotalWaves => _runtimeWaves?.Count ?? 0;

        private readonly EnemyPool _pool = new();
        private readonly List<GameObject> _spawnBag = new();
        private List<WaveDefinition> _runtimeWaves;
        private Coroutine _runRoutine;

        private void OnEnable()
        {
            _runRoutine = StartCoroutine(RunWaves());
        }

        private void OnDisable()
        {
            if (_runRoutine != null)
            {
                StopCoroutine(_runRoutine);
                _runRoutine = null;
            }
        }

        private IEnumerator RunWaves()
        {
            BuildRuntimeWaves();

            if (TotalWaves == 0)
            {
                Debug.LogWarning($"{nameof(WaveSpawner)}: no waves configured (assign a JSON or a WaveData).", this);
                yield break;
            }

            // Resolve where to resume BEFORE any yield: the continue flag is consumed a frame later
            // by GameSaveCoordinator.
            int startIndex = ResolveStartWaveIndex();

            // Wait until the player exists before spawning anything around it.
            while (GameManagerScript.Instance == null || GameManagerScript.Instance.Player == null)
                yield return null;

            for (int i = startIndex; i < TotalWaves; i++)
            {
                if (GameManagerScript.Instance.IsGameOver)
                    yield break;

                yield return SpawnWave(i);
            }

            OnAllWavesCompleted?.Invoke();
        }

        private int ResolveStartWaveIndex()
        {
            if (!SaveSystem.ContinueRequested)
                return 0;

            GameSaveData data = SaveSystem.Load();
            if (data == null)
                return 0;

            return Mathf.Clamp(data.currentWave, 0, TotalWaves - 1);
        }

        private IEnumerator SpawnWave(int index)
        {
            WaveDefinition wave = _runtimeWaves[index];
            CurrentWaveNumber = index + 1;

            if (wave.startDelay > 0f)
                yield return new WaitForSeconds(wave.startDelay);

            // Fires the wave-start save (GameSaveCoordinator listens here).
            OnWaveStarted?.Invoke(CurrentWaveNumber, TotalWaves);

            BuildBag(wave);

            float interval = Mathf.Max(0f, wave.spawnInterval);
            while (_spawnBag.Count > 0)
            {
                if (GameManagerScript.Instance.IsGameOver)
                    yield break;

                GameObject prefab = _spawnBag[^1];
                _spawnBag.RemoveAt(_spawnBag.Count - 1);
                Spawn(prefab);

                if (interval > 0f)
                    yield return new WaitForSeconds(interval);
                else
                    yield return null;
            }

            // Wave isn't complete until every enemy it spawned is dead.
            while (GameManagerScript.Instance.TotalEnemiesAlive > 0)
            {
                if (GameManagerScript.Instance.IsGameOver)
                    yield break;

                yield return null;
            }

            OnWaveCompleted?.Invoke(CurrentWaveNumber);
        }

        // ---- Wave source --------------------------------------------------

        private void BuildRuntimeWaves()
        {
            _runtimeWaves = new List<WaveDefinition>();

            if (wavesJson != null && !string.IsNullOrWhiteSpace(wavesJson.text))
            {
                BuildFromJson(wavesJson.text);
                return;
            }

            if (waveData != null && waveData.waves != null)
                _runtimeWaves.AddRange(waveData.waves);
        }

        private void BuildFromJson(string json)
        {
            WaveConfigJson config;
            try
            {
                config = JsonUtility.FromJson<WaveConfigJson>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(WaveSpawner)}: could not parse waves JSON — {e.Message}", this);
                return;
            }

            if (config?.waves == null)
                return;

            Dictionary<string, GameObject> catalog = BuildCatalog();

            foreach (WaveJson w in config.waves)
            {
                if (w == null)
                    continue;

                WaveDefinition def = new WaveDefinition
                {
                    waveName = w.waveName,
                    spawnInterval = w.spawnInterval,
                    startDelay = w.startDelay,
                    spawnEntries = new List<SpawnEntry>()
                };

                if (w.entries != null)
                {
                    foreach (WaveEntryJson e in w.entries)
                    {
                        if (e == null || string.IsNullOrWhiteSpace(e.enemy) || e.amount <= 0)
                            continue;

                        if (!catalog.TryGetValue(e.enemy, out GameObject prefab) || prefab == null)
                        {
                            Debug.LogWarning($"{nameof(WaveSpawner)}: enemy id '{e.enemy}' is not in the catalog — skipped.", this);
                            continue;
                        }

                        def.spawnEntries.Add(new SpawnEntry { prefab = prefab, amount = e.amount });
                    }
                }

                _runtimeWaves.Add(def);
            }
        }

        private Dictionary<string, GameObject> BuildCatalog()
        {
            var dict = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

            foreach (EnemyCatalogEntry entry in enemyCatalog)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || entry.prefab == null)
                    continue;

                dict[entry.id] = entry.prefab;
            }

            return dict;
        }

        // ---- Spawning -----------------------------------------------------

        private void BuildBag(WaveDefinition wave)
        {
            _spawnBag.Clear();

            if (wave.spawnEntries == null)
                return;

            foreach (var entry in wave.spawnEntries)
            {
                if (entry == null || entry.prefab == null || entry.amount <= 0)
                    continue;

                for (int i = 0; i < entry.amount; i++)
                    _spawnBag.Add(entry.prefab);
            }

            // Shuffle so mixed-roster waves don't spawn in blocks.
            for (int i = 0; i < _spawnBag.Count; i++)
            {
                int rand = UnityEngine.Random.Range(i, _spawnBag.Count);
                (_spawnBag[i], _spawnBag[rand]) = (_spawnBag[rand], _spawnBag[i]);
            }
        }

        private void Spawn(GameObject prefab)
        {
            Vector3 spawnPos = GetRandomPositionAroundPlayer();

            if (useEnemyPooling)
                _pool.Get(prefab, spawnPos);
            else
                Instantiate(prefab, spawnPos, Quaternion.identity);
        }

        private Vector3 GetRandomPositionAroundPlayer()
        {
            Vector3 playerPos = GameManagerScript.Instance.Player.position;
            Vector2 circle = UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
            return playerPos + new Vector3(circle.x, circle.y, 0f);
        }
    }
}
