using Scores;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>A single row in the leaderboard list. Put this on the row prefab.</summary>
    public class LeaderboardRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text detailText;

        public void Bind(int rank, LeaderboardEntry entry)
        {
            if (rankText != null)
                rankText.text = $"{rank}";

            if (timeText != null)
                timeText.text = SurvivalTimer.Format(entry.seconds);

            if (detailText != null)
                detailText.text = $"Lv {entry.level} · {entry.coins} coins";
        }
    }
}
