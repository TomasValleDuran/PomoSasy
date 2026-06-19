using System.Collections.Generic;
using System.Text;
using Scores;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Renders the high-score table. Two ways to display it (pick one in the Inspector):
    ///  • Assign Row Prefab + Content to build a styled row per entry (recommended).
    ///  • Or assign Fallback Text for a quick single multi-line list (no prefab needed).
    /// Refreshes whenever the panel is enabled.
    /// </summary>
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("Row mode (preferred)")]
        [Tooltip("Parent the rows are instantiated under (e.g. a Vertical Layout Group).")]
        [SerializeField] private Transform content;
        [SerializeField] private LeaderboardRowView rowPrefab;

        [Header("Simple mode (fallback)")]
        [Tooltip("If set and no row prefab is assigned, the whole table is dumped here.")]
        [SerializeField] private TMP_Text fallbackText;

        [Header("Optional")]
        [Tooltip("Shown when there are no scores yet.")]
        [SerializeField] private GameObject emptyState;

        private readonly List<GameObject> _spawnedRows = new();

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            IReadOnlyList<LeaderboardEntry> entries = LeaderboardSystem.GetEntries();

            if (emptyState != null)
                emptyState.SetActive(entries.Count == 0);

            if (rowPrefab != null && content != null)
                BuildRows(entries);
            else if (fallbackText != null)
                BuildText(entries);
        }

        private void BuildRows(IReadOnlyList<LeaderboardEntry> entries)
        {
            foreach (GameObject row in _spawnedRows)
                Destroy(row);
            _spawnedRows.Clear();

            for (int i = 0; i < entries.Count; i++)
            {
                LeaderboardRowView row = Instantiate(rowPrefab, content);
                row.Bind(i + 1, entries[i]);
                _spawnedRows.Add(row.gameObject);
            }
        }

        private void BuildText(IReadOnlyList<LeaderboardEntry> entries)
        {
            if (entries.Count == 0)
            {
                fallbackText.text = "No scores yet.";
                return;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < entries.Count; i++)
            {
                LeaderboardEntry e = entries[i];
                sb.AppendLine($"{i + 1}.  {SurvivalTimer.Format(e.seconds)}   Lv {e.level}   {e.coins} coins");
            }

            fallbackText.text = sb.ToString();
        }
    }
}
