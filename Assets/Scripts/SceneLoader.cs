using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader _instance;

    /// <summary>
    /// Self-healing singleton: if no SceneLoader exists yet (e.g. you pressed Play directly in the
    /// Gameplay scene instead of starting from MainMenu), one is created on demand so callers never
    /// hit a null Instance.
    /// </summary>
    public static SceneLoader Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameObject(nameof(SceneLoader)).AddComponent<SceneLoader>();

            return _instance;
        }
    }

    public void Awake()
    {
        // Read the backing field (not the property) to avoid recursively creating an instance.
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(int index)
    {
        if (GameManagerScript.Instance != null)
            GameManagerScript.Instance.ResetGameState();

        UnityEngine.SceneManagement.SceneManager.LoadScene(index);
    }
    
    public void LoadMainMenu()
    {
        LoadScene(0);
    }
    public void LoadGameplay()
    {
        LoadScene(1);
    }
}