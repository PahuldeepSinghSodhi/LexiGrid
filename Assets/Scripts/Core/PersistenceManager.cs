using UnityEngine;
using System.IO;
using System;
using LexiGrid.DataStructures;

namespace LexiGrid.Core
{
    /// <summary>
    /// Handles saving and loading leaderboard data to JSON.
    /// </summary>
    public static class PersistenceManager
    {
        [Serializable]
        private class LeaderboardData
        {
            public string[] PlayerNames;
            public int[] Scores;
            public string[] Dates;
        }

        private static string SavePath => Path.Combine(Application.persistentDataPath, "leaderboard.json");

        /// <summary>
        /// Saves the leaderboard data to a JSON file.
        /// </summary>
        /// <param name="heap">The MinHeap containing leaderboard entries.</param>
        public static void SaveLeaderboard(MinHeap<LeaderboardEntry> heap)
        {
            try
            {
                var entries = heap.ToArray();
                LeaderboardData data = new LeaderboardData
                {
                    PlayerNames = new string[entries.Length],
                    Scores = new int[entries.Length],
                    Dates = new string[entries.Length]
                };

                for (int i = 0; i < entries.Length; i++)
                {
                    data.PlayerNames[i] = entries[i].PlayerName;
                    data.Scores[i] = entries[i].Score;
                    data.Dates[i] = entries[i].Date;
                }

                string json = JsonUtility.ToJson(data);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save leaderboard: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the leaderboard data from a JSON file.
        /// </summary>
        /// <returns>A MinHeap populated with the loaded leaderboard entries.</returns>
        public static MinHeap<LeaderboardEntry> LoadLeaderboard()
        {
            MinHeap<LeaderboardEntry> heap = new MinHeap<LeaderboardEntry>();

            if (HasSaveData())
            {
                try
                {
                    string json = File.ReadAllText(SavePath);
                    LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);

                    if (data != null && data.PlayerNames != null)
                    {
                        for (int i = 0; i < data.PlayerNames.Length; i++)
                        {
                            heap.Insert(new LeaderboardEntry(data.PlayerNames[i], data.Scores[i], data.Dates[i]));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to load leaderboard: {ex.Message}");
                }
            }

            return heap;
        }

        /// <summary>
        /// Checks if leaderboard save data exists.
        /// </summary>
        /// <returns>True if save data exists, false otherwise.</returns>
        public static bool HasSaveData()
        {
            return File.Exists(SavePath);
        }

        /// <summary>
        /// Deletes the leaderboard save data if it exists.
        /// </summary>
        public static void DeleteSaveData()
        {
            if (HasSaveData())
            {
                try
                {
                    File.Delete(SavePath);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to delete save data: {ex.Message}");
                }
            }
        }
    }
}
