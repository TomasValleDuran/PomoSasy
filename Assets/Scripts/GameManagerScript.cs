using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript Instance { get; private set; }

    public Transform Player { get; private set; }

    private readonly Dictionary<string, int> _enemyCounts = new();
    private int _pauseRequestCount;

    public bool IsPaused => _pauseRequestCount > 0;
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            _pauseRequestCount = 0;
            Time.timeScale = 1f;
        }
    }

    public void RegisterPlayer(Transform player)
    {
        Player = player;
    }

    public void RegisterEnemy(string enemyType)
    {
        if (string.IsNullOrWhiteSpace(enemyType))
        {
            enemyType = "Unknown";
        }

        if (_enemyCounts.ContainsKey(enemyType))
        {
            _enemyCounts[enemyType]++;
            return;
        }

        _enemyCounts[enemyType] = 1;
    }

    public void UnregisterEnemy(string enemyType)
    {
        if (string.IsNullOrWhiteSpace(enemyType) || !_enemyCounts.ContainsKey(enemyType))
        {
            return;
        }

        _enemyCounts[enemyType]--;

        if (_enemyCounts[enemyType] <= 0)
        {
            _enemyCounts.Remove(enemyType);
        }
    }

    public int GetEnemyCount(string enemyType)
    {
        if (string.IsNullOrWhiteSpace(enemyType))
        {
            return 0;
        }

        return _enemyCounts.TryGetValue(enemyType, out int count) ? count : 0;
    }

    public IReadOnlyDictionary<string, int> GetAllEnemyCounts()
    {
        return _enemyCounts;
    }

    public void SetGameOver()
    {
        IsGameOver = true;
    }

    public void ResetGameState()
    {
        IsGameOver = false;
        _pauseRequestCount = 0;
        Time.timeScale = 1f;
    }

    public void RequestPause()
    {
        _pauseRequestCount++;
        ApplyPauseState();
    }

    public void ReleasePause()
    {
        _pauseRequestCount = Mathf.Max(0, _pauseRequestCount - 1);
        ApplyPauseState();
    }

    private void ApplyPauseState()
    {
        Time.timeScale = IsPaused ? 0f : 1f;
    }
}
