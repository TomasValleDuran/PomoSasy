using UnityEngine;

namespace UI
{
    public class GameplayUI : MonoBehaviour
    {
        public void ExitGame()
        {
            SceneLoader.Instance.LoadMainMenu();
        }
    }
}