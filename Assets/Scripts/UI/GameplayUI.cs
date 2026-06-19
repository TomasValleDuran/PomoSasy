using Controllers;
using Health;
using Save;
using Scores;
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
        [SerializeField] private TMP_Text survivedTimeText;
        [Tooltip("Optional: shown on the game-over screen when the run made the leaderboard top 10.")]
        [SerializeField] private TMP_Text gameOverRankText;
        [SerializeField] private AudioClip uiWindowOpenSfx;
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip buttonClickSfx;
         [Range(0f, 1f)]
        [Header("Victory stats (optional)")]
        [SerializeField] private TMP_Text victoryLevelText;
        [SerializeField] private TMP_Text victoryMoneyText;
        [SerializeField] private TMP_Text victoryTimeText;
        [SerializeField] private TMP_Text victoryRankText;

        private bool _pauseRequestedByUi;
        private bool _pauseRequestedByGameOver;
        private bool _isGameOver;
        private HealthComponent _playerHealth;
        private PlayerController _playerController;
        private PlayerAttacker _playerAttacker;
        private WaveSpawner _waveSpawner;

        private void Awake()
        {
            UIAudioPlayer.ConfigureButtonClick(buttonClickSfx, 0.5f);

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

            int rank = RecordRun();
            PopulateVictoryStats(rank);

            DialogAnimator.Set(victoryDialog, true);

            RequestGameOverPause();
        }

        private void PopulateVictoryStats(int rank)
        {
            if (XpManagerScript.Instance != null && victoryLevelText != null)
                victoryLevelText.text = $"Reached Level {XpManagerScript.Instance.CurrentLevel}";

            if (WalletManagerScript.Instance != null && victoryMoneyText != null)
                victoryMoneyText.text = $"Collected {WalletManagerScript.Instance.CurrentMoney:N0} Coins";

            if (victoryTimeText != null && SurvivalTimer.Instance != null)
                victoryTimeText.text = SurvivalTimer.Format(SurvivalTimer.Instance.ElapsedSeconds);

            ApplyRankText(victoryRankText, rank);
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

            int rank = RecordRun();
            PopulateGameOverStats(rank);

            DialogAnimator.Set(gameOverDialog, true);

            RequestGameOverPause();
        }

        private void PopulateGameOverStats(int rank)
        {
            if (XpManagerScript.Instance != null)
            {
                if (levelText != null)
                    levelText.text = $"Reached Level {XpManagerScript.Instance.CurrentLevel}";

                if (totalXpText != null)
                    totalXpText.text = $"Earned {XpManagerScript.Instance.CurrentXp:N0} XP";
            }

            if (WalletManagerScript.Instance != null && moneyText != null)
                moneyText.text = $"Collected {WalletManagerScript.Instance.CurrentMoney:N0} Coins";

            if (survivedTimeText != null && SurvivalTimer.Instance != null)
                survivedTimeText.text = SurvivalTimer.Format(SurvivalTimer.Instance.ElapsedSeconds);

            ApplyRankText(gameOverRankText, rank);
        }

        /// <summary>Saves this run to the leaderboard and returns its rank (1-based, or -1 if unranked).</summary>
        private int RecordRun()
        {
            float seconds = SurvivalTimer.Instance != null ? SurvivalTimer.Instance.ElapsedSeconds : 0f;
            int level = XpManagerScript.Instance != null ? XpManagerScript.Instance.CurrentLevel : 0;
            int coins = WalletManagerScript.Instance != null ? WalletManagerScript.Instance.CurrentMoney : 0;

            return LeaderboardSystem.Record(seconds, level, coins);
        }

        private static void ApplyRankText(TMP_Text target, int rank)
        {
            if (target == null)
                return;

            if (rank == 1)
                target.text = "New best!";
            else if (rank > 0)
                target.text = $"Leaderboard #{rank}";
            else
                target.text = string.Empty;
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

            DialogAnimator.Set(pauseDialog, paused);

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
