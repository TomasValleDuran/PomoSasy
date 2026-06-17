using System;
using System.IO;
using UnityEngine;

namespace Save
{
    /// <summary>
    /// Static facade over the save file on disk plus the cross-scene "continue requested" flag.
    /// The actual capture/restore of game state lives in <see cref="GameSaveCoordinator"/>.
    /// </summary>
    public static class SaveSystem
    {
        private const string FileName = "savegame.json";

        /// <summary>Set by the main menu when the player picks "Continue", consumed once in Gameplay.</summary>
        public static bool ContinueRequested { get; set; }

        private static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public static bool HasSave => File.Exists(SavePath);

        public static void Save(GameSaveData data)
        {
            if (data == null)
                return;

            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Could not write save: {e.Message}");
            }
        }

        public static GameSaveData Load()
        {
            if (!File.Exists(SavePath))
                return null;

            try
            {
                string json = File.ReadAllText(SavePath);
                return JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Could not read save: {e.Message}");
                return null;
            }
        }

        public static void Delete()
        {
            try
            {
                if (File.Exists(SavePath))
                    File.Delete(SavePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Could not delete save: {e.Message}");
            }
        }
    }
}
