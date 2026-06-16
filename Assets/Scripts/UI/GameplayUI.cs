using Controllers;
using Health;
using TMPro;
using UnityEngine;

namespace UI
{
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField] private GameObject pauseDialog;
        [SerializeField] private GameObject gameOverDialog;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text totalXpText;
        [SerializeField] private TMP_Text moneyText;

        private bool _pauseRequestedByUi;
        private bool _pauseRequestedByGameOver;
        private bool _isGameOver;
        private HealthComponent _playerHealth;
        private PlayerController _playerController;
        private PlayerAttacker _playerAttacker;

        private void Awake()
        {
            if (pauseDialog != null)
                pauseDialog.SetActive(false);

            if (gameOverDialog != null)
                gameOverDialog.SetActive(false);
        }

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null || !player.TryGetComponent(out _playerHealth))
                return;

            _playerHealth.OnDeath += HandlePlayerDeath;
        }

        private void OnDestroy()
        {
            if (_playerHealth != null)
                _playerHealth.OnDeath -= HandlePlayerDeath;

            ReleaseGameOverPause();
        }

        public void OpenPause()
        {
            if (_isGameOver)
                return;

            SetPause(true);
        }

        public void ClosePause()
        {
            SetPause(false);
        }

        public void TogglePause()
        {
            if (_isGameOver)
                return;

            SetPause(!_pauseRequestedByUi);
        }

        public void ExitGame()
        {
            SetPause(false);
            SceneLoader.Instance.LoadMainMenu();
        }

        public void ExitToMainMenuFromGameOver()
        {
            ReleaseGameOverPause();
            SceneLoader.Instance.LoadMainMenu();
        }

        private void HandlePlayerDeath()
        {
            if (_isGameOver)
                return;

            _isGameOver = true;
            GameManagerScript.Instance?.SetGameOver();

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
