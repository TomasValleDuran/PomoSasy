using System;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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