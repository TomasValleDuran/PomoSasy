using Controllers;
using Health;
using Save;
using Spawner;
using TMPro;
using UnityEngine;

namespace UI
{
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField] private GameObject pauseDialog;
        [SerializeField] private GameObject gameOverDialog;
        [SerializeField] private GameObject victoryDialog;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text totalXpText;
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private AudioClip uiWindowOpenSfx;
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip buttonClickSfx;
        [SerializeField] [Range(0f, 1f)] private float buttonClickVolume = 0.8f;

        [Header("Victory stats (optional)")]
        [SerializeField] private TMP_Text victoryLevelText;
        [SerializeField] private TMP_Text victoryMoneyText;

        private bool _pauseRequestedByUi;
        private bool _pauseRequestedByGameOver;
        private bool _isGameOver;
        private HealthComponent _playerHealth;
        private PlayerController _playerController;
        private PlayerAttacker _playerAttacker;
        private WaveSpawner _waveSpawner;

        private void Awake()
        {
            UIAudioPlayer.ConfigureButtonClick(buttonClickSfx, buttonClickVolume);

            if (pauseDialog != null)
                pauseDialog.SetActive(false);

            if (gameOverDialog != null)
                gameOverDialog.SetActive(false);

            if (victoryDialog != null)
                victoryDialog.SetActive(false);
        }

        private void Start()
        {
            _waveSpawner = FindFirstObjectByType<WaveSpawner>();
            if (_waveSpawner != null)
                _waveSpawner.OnAllWavesCompleted += HandleVictory;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null || !player.TryGetComponent(out _playerHealth))
                return;

            _playerHealth.OnDeath += HandlePlayerDeath;
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
                _playerHealth.OnDeath -= HandlePlayerDeath;

            if (_waveSpawner != null)
                _waveSpawner.OnAllWavesCompleted -= HandleVictory;

            ReleaseGameOverPause();
        }

        public void OpenPause()
        {
            UIAudioPlayer.PlayButtonClick();

            if (_isGameOver)
                return;

            SetPause(true);
            // No save here on purpose: the run is only persisted at the START of each wave
            // (see GameSaveCoordinator) so partial-wave progress can't be farmed by quitting.
        }

        public void ClosePause()
        {
            UIAudioPlayer.PlayButtonClick();
            SetPause(false);
        }

        public void TogglePause()
        {
            UIAudioPlayer.PlayButtonClick();

            if (_isGameOver)
                return;

            bool willPause = !_pauseRequestedByUi;
            SetPause(willPause);
        }

        public void ExitGame()
        {
            UIAudioPlayer.PlayButtonClick();

            // The save is the snapshot from the start of the current wave; leave it untouched.
            SetPause(false);
            SceneLoader.Instance.LoadMainMenu();
        }

        public void ExitToMainMenuFromGameOver()
        {
            UIAudioPlayer.PlayButtonClick();
            ReleaseGameOverPause();
            SceneLoader.Instance.LoadMainMenu();
        }

        public void ExitToMainMenuFromVictory()
        {
            UIAudioPlayer.PlayButtonClick();
            ReleaseGameOverPause();
            SceneLoader.Instance.LoadMainMenu();
        }

        private void HandleVictory()
        {
            if (_isGameOver)
                return;

            _isGameOver = true;
            GameManagerScript.Instance?.SetGameOver();

            // The run is over (won) — drop the save so "Continue" won't reload a finished run.
            SaveSystem.Delete();

            SetPause(false);
            SetPlayerControlBlocked(true);
            PopulateVictoryStats();

            if (victoryDialog != null)
                victoryDialog.SetActive(true);

            RequestGameOverPause();
        }

        private void PopulateVictoryStats()
        {
            if (XpManagerScript.Instance != null && victoryLevelText != null)
                victoryLevelText.text = $"You reached level {XpManagerScript.Instance.CurrentLevel}.";

            if (WalletManagerScript.Instance != null && victoryMoneyText != null)
                victoryMoneyText.text = $"You earned {WalletManagerScript.Instance.CurrentMoney} coins.";
        }

        private void HandlePlayerDeath()
        {
            if (_isGameOver)
                return;

            _isGameOver = true;
            GameManagerScript.Instance?.SetGameOver();

            // The run is over — drop the save so "Continue" won't load a dead player.
            SaveSystem.Delete();

            SetPause(false);
            SetPlayerControlBlocked(true);
            PopulateGameOverStats();

            if (gameOverDialog != null)
                gameOverDialog.SetActive(true);

            RequestGameOverPause();
        }

        private void PopulateGameOverStats()
        {
            if (XpManagerScript.Instance != null)
            {
                if (levelText != null)
                    levelText.text = $"You reached level {XpManagerScript.Instance.CurrentLevel}.";

                if (totalXpText != null)
                    totalXpText.text = $"You got a total of {XpManagerScript.Instance.CurrentXp} experience.";
            }

            if (WalletManagerScript.Instance != null && moneyText != null)
                moneyText.text = $"You earned {WalletManagerScript.Instance.CurrentMoney} coins.";
        }

        private void SetPlayerControlBlocked(bool blocked)
        {
            if (GameManagerScript.Instance == null || GameManagerScript.Instance.Player == null)
                return;

            if (_playerController == null)
                GameManagerScript.Instance.Player.TryGetComponent(out _playerController);
            if (_playerAttacker == null)
                GameManagerScript.Instance.Player.TryGetComponent(out _playerAttacker);

            if (_playerController != null)
                _playerController.enabled = !blocked;
            if (_playerAttacker != null)
                _playerAttacker.enabled = !blocked;
        }

        private void SetPause(bool paused)
        {
            UIAudioPlayer.Play(uiAudioSource, uiWindowOpenSfx, 0.5f);
            if (GameManagerScript.Instance == null)
                return;

            if (pauseDialog != null)
                pauseDialog.SetActive(paused);

            if (paused)
            {
                if (_pauseRequestedByUi)
                    return;

                GameManagerScript.Instance.RequestPause();
                _pauseRequestedByUi = true;
                return;
            }

            if (!_pauseRequestedByUi)
                return;
            GameManagerScript.Instance.ReleasePause();
            _pauseRequestedByUi = false;
        }

        private void RequestGameOverPause()
        {
            if (GameManagerScript.Instance == null || _pauseRequestedByGameOver)
                return;

            GameManagerScript.Instance.RequestPause();
            _pauseRequestedByGameOver = true;
        }

        private void ReleaseGameOverPause()
        {
            if (GameManagerScript.Instance == null || !_pauseRequestedByGameOver)
                return;

            GameManagerScript.Instance.ReleasePause();
            _pauseRequestedByGameOver = false;
        }
    }
}
