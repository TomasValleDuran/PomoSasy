using System.Collections;
using Spawner;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Shows the current wave ("Wave X / N") and the number of enemies left to clear it.
    /// Binds to a <see cref="WaveSpawner"/> in the scene.
    /// </summary>
    public class WaveUI : MonoBehaviour
    {
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private TMP_Text enemiesText;

        [Header("Wave banner (optional)")]
        [Tooltip("Shown briefly when a new wave starts. Leave empty to disable.")]
        [SerializeField] private GameObject waveBanner;
        [SerializeField] private TMP_Text waveBannerText;
        [SerializeField] private float bannerSeconds = 2f;

        private Coroutine _bannerRoutine;

        private void Start()
        {
            if (waveSpawner == null)
                waveSpawner = FindFirstObjectByType<WaveSpawner>();

            if (waveSpawner != null)
            {
                waveSpawner.OnWaveStarted += HandleWaveStarted;
                waveSpawner.OnAllWavesCompleted += HandleAllWavesCompleted;
            }

            if (GameManagerScript.Instance != null)
                GameManagerScript.Instance.OnEnemyCountChanged += UpdateEnemiesLeft;

            if (waveBanner != null)
                waveBanner.SetActive(false);

            UpdateEnemiesLeft();
        }

        private void OnDestroy()
        {
            if (waveSpawner != null)
            {
                waveSpawner.OnWaveStarted -= HandleWaveStarted;
                waveSpawner.OnAllWavesCompleted -= HandleAllWavesCompleted;
            }

            if (GameManagerScript.Instance != null)
                GameManagerScript.Instance.OnEnemyCountChanged -= UpdateEnemiesLeft;
        }

        private void HandleWaveStarted(int waveNumber, int totalWaves)
        {
            if (waveText != null)
                waveText.text = $"Wave {waveNumber} / {totalWaves}";

            ShowBanner($"Wave {waveNumber}");
            UpdateEnemiesLeft();
        }

        private void HandleAllWavesCompleted()
        {
            if (waveText != null)
                waveText.text = "All waves cleared!";

            if (enemiesText != null)
                enemiesText.text = string.Empty;
        }

        private void UpdateEnemiesLeft()
        {
            if (enemiesText == null || GameManagerScript.Instance == null)
                return;

            enemiesText.text = $"Enemies: {GameManagerScript.Instance.TotalEnemiesAlive}";
        }

        private void ShowBanner(string message)
        {
            if (waveBanner == null)
                return;

            if (waveBannerText != null)
                waveBannerText.text = message;

            if (_bannerRoutine != null)
                StopCoroutine(_bannerRoutine);

            _bannerRoutine = StartCoroutine(BannerRoutine());
        }

        private IEnumerator BannerRoutine()
        {
            waveBanner.SetActive(true);
            yield return new WaitForSeconds(bannerSeconds);
            waveBanner.SetActive(false);
            _bannerRoutine = null;
        }
    }
}
