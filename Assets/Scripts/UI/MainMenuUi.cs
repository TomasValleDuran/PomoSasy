using UnityEngine;

namespace UI
{

    public class MainMenuUI : MonoBehaviour
    {
        public void ContinueGame()
        {
            
            SceneLoader.Instance.LoadGameplay();
        }

        public void NewGame()
        {
            SceneLoader.Instance.LoadGameplay();
        }
    }
}