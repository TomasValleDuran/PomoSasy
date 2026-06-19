using UnityEngine;

/// <summary>
/// Counts how long the current run has lasted. Stops automatically while the game
/// is paused or over. Lives in the Gameplay scene (not DontDestroyOnLoad) so it
/// resets to zero on every new run. Read <see cref="ElapsedSeconds"/> at game over
/// to feed the leaderboard.
/// </summary>
public class SurvivalTimer : MonoBehaviour
{
    public static SurvivalTimer Instance { get; private set; }

    public float ElapsedSeconds { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        var gm = GameManagerScript.Instance;
        if (gm != null && (gm.IsPaused || gm.IsGameOver))
            return;

        ElapsedSeconds += Time.deltaTime;
    }

    /// <summary>Resumes the clock from a saved value (used when continuing a run).</summary>
    public void Restore(float seconds)
    {
        ElapsedSeconds = Mathf.Max(0f, seconds);
    }

    /// <summary>Formats seconds as mm:ss (e.g. 754 -> "12:34").</summary>
    public static string Format(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        int total = Mathf.FloorToInt(seconds);
        return $"{total / 60:00}:{total % 60:00}";
    }
}
