using System.IO;
using UnityEngine;
using IdleViking.Models;

namespace IdleViking.Core
{
    /// <summary>
    /// Handles save/load of GameState to JSON on disk.
    /// Uses Application.persistentDataPath so it works on Android.
    /// </summary>
    public static class SaveSystem
    {
        private const string SAVE_FILE = "save.json";

        private static string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE);

        public static void Save(GameState state)
        {
            state.MarkSaveTime();
            string json = JsonUtility.ToJson(state, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[SaveSystem] Game saved to {SavePath}");
        }

        public static GameState Load()
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("[SaveSystem] No save file found, creating new game state.");
                return new GameState();
            }

            string json = File.ReadAllText(SavePath);
            GameState state = JsonUtility.FromJson<GameState>(json);

            if (state == null)
            {
                Debug.LogWarning("[SaveSystem] Failed to parse save file, creating new game state.");
                return new GameState();
            }

            Debug.Log("[SaveSystem] Game loaded successfully.");
            return state;
        }

        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("[SaveSystem] Save file deleted.");
            }
        }

        public static bool HasSave()
        {
            return File.Exists(SavePath);
        }
    }
}
