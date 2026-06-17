using Save;
using UnityEngine;

namespace UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Tooltip("The 'Continue' button. It is shown only when a saved game exists.")]
        [SerializeField] private GameObject continueButton;

        private void Start()
        {
            RefreshContinueButton();
        }

        private void OnEnable()
        {
            RefreshContinueButton();
        }

        /// <summary>Resume the saved run (or start fresh if, somehow, there is no save).</summary>
        public void ContinueGame()
        {
            if (!SaveSystem.HasSave)
            {
                NewGame();
                return;
            }

            SaveSystem.ContinueRequested = true;
            SceneLoader.Instance.LoadGameplay();
        }

        /// <summary>Start from scratch: drop any existing save so nothing is restored.</summary>
        public void NewGame()
        {
            SaveSystem.ContinueRequested = false;
            SaveSystem.Delete();
            SceneLoader.Instance.LoadGameplay();
        }

        /// <summary>Placeholder for the settings screen. Does nothing for now.</summary>
        public void OpenSettings()
        {
            Debug.Log("Settings pressed (not implemented yet).");
        }

        /// <summary>Show the Continue button only if there is a saved game to resume.</summary>
        private void RefreshContinueButton()
        {
            if (continueButton != null)
                continueButton.SetActive(SaveSystem.HasSave);
        }
    }
}
