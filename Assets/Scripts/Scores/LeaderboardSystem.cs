using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Scores
{
    /// <summary>One recorded run. Ranked primarily by survival time.</summary>
    [Serializable]
    public class LeaderboardEntry
    {
        public float seconds;
        public int level;
        public int coins;
        public string dateUtc;
    }

    [Serializable]
    public class LeaderboardData
    {
        public List<LeaderboardEntry> entries = new();
    }

    /// <summary>
    /// Local high-score table persisted to its own JSON file (separate from the save game, so it
    /// survives "New Game" and death). Ranked by time survived, then level, then coins. Keeps the
    /// top <see cref="MaxEntries"/>.
    /// </summary>
    public static class LeaderboardSystem
    {
        private const string FileName = "leaderboard.json";
        public const int MaxEntries = 10;

        private static string Path_ => Path.Combine(Application.persistentDataPath, FileName);

        /// <summary>
        /// Records a finished run. Returns its 1-based rank in the table, or -1 if it didn't make
        /// the top <see cref="MaxEntries"/>. Rank 1 means a new best.
        /// </summary>
        public static int Record(float seconds, int level, int coins)
        {
            LeaderboardData data = Load();

            var entry = new LeaderboardEntry
            {
                seconds = seconds,
                level = level,
                coins = coins,
                dateUtc = DateTime.UtcNow.ToString("o")
            };

            data.entries.Add(entry);
            data.entries.Sort(Compare);

            int rank = data.entries.IndexOf(entry) + 1;

            if (data.entries.Count > MaxEntries)
                data.entries.RemoveRange(MaxEntries, data.entries.Count - MaxEntries);

            Save(data);

            return rank <= MaxEntries ? rank : -1;
        }

        public static IReadOnlyList<LeaderboardEntry> GetEntries()
        {
            return Load().entries;
        }

        public static void Clear()
        {
            try
            {
                if (File.Exists(Path_))
                    File.Delete(Path_);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Leaderboard] Could not clear: {e.Message}");
            }
        }

        private static int Compare(LeaderboardEntry a, LeaderboardEntry b)
        {
            int byTime = b.seconds.CompareTo(a.seconds);
            if (byTime != 0) return byTime;

            int byLevel = b.level.CompareTo(a.level);
            if (byLevel != 0) return byLevel;

            return b.coins.CompareTo(a.coins);
        }

        private static LeaderboardData Load()
        {
            if (!File.Exists(Path_))
                return new LeaderboardData();

            try
            {
                string json = File.ReadAllText(Path_);
                return JsonUtility.FromJson<LeaderboardData>(json) ?? new LeaderboardData();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Leaderboard] Could not read: {e.Message}");
                return new LeaderboardData();
            }
        }

        private static void Save(LeaderboardData data)
        {
            try
            {
                File.WriteAllText(Path_, JsonUtility.ToJson(data, true));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Leaderboard] Could not write: {e.Message}");
            }
        }
    }
}
