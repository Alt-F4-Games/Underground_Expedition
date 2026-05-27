using System.IO;
using Fusion;
using Network.Quests.Runtime;
using UnityEngine;

namespace Network.Quests.Save
{
    public static class NetworkQuestSaveSystem
    {
        // =====================================================
        // PATH
        // =====================================================

        private static string GetPath(
            PlayerRef player)
        {
            return Path.Combine(
                Application.persistentDataPath,
                $"quests_{player.PlayerId}.json");
        }

        // =====================================================
        // SAVE
        // =====================================================

        public static void Save(
            PlayerRef player,
            NetworkQuestManager manager)
        {
            if (manager == null)
                return;

            QuestSyncData data =
                manager.CreateSyncData();

            string json =
                JsonUtility.ToJson(
                    data,
                    true);

            File.WriteAllText(
                GetPath(player),
                json);

            Debug.Log(
                $"[QuestSave] Saved player quests: {player.PlayerId}");
        }

        // =====================================================
        // LOAD
        // =====================================================

        public static void Load(
            PlayerRef player,
            NetworkQuestManager manager)
        {
            string path =
                GetPath(player);

            if (!File.Exists(path))
            {
                Debug.Log(
                    $"[QuestSave] No save found for player {player.PlayerId}");

                return;
            }

            string json =
                File.ReadAllText(path);

            QuestSyncData data =
                JsonUtility.FromJson<QuestSyncData>(
                    json);

            if (data == null)
                return;

            manager.LoadFromSyncData(
                data);

            Debug.Log(
                $"[QuestSave] Loaded player quests: {player.PlayerId}");
        }
    }
}