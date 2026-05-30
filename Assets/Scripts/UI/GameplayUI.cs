using UnityEngine;

namespace UI
{
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField] private GameObject pauseDialog;

        private bool _pauseRequestedByUi;

        private void Awake()
        {
            if (pauseDialog != null)
                pauseDialog.SetActive(false);
        }

        public void OpenPause()
        {
            SetPause(true);
        }

        public void ClosePause()
        {
            SetPause(false);
        }

        public void TogglePause()
        {
            SetPause(!_pauseRequestedByUi);
        }

        public void ExitGame()
        {
            SetPause(false);
            SceneLoader.Instance.LoadMainMenu();
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
    }
}
