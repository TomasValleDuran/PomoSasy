using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>Displays the run clock as mm:ss. Reads from <see cref="SurvivalTimer"/>.</summary>
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;

        private int _lastShownSeconds = -1;

        private void Update()
        {
            if (timerText == null || SurvivalTimer.Instance == null)
                return;

            // Only rebuild the text when the displayed second changes (avoids per-frame TMP work).
            int seconds = Mathf.FloorToInt(SurvivalTimer.Instance.ElapsedSeconds);
            if (seconds == _lastShownSeconds)
                return;

            _lastShownSeconds = seconds;
            timerText.text = SurvivalTimer.Format(seconds);
        }
    }
}
