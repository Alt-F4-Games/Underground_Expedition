using System.IO;
using Fusion;
using Network.Quests.Runtime;
using UnityEngine;

namespace Network.Quests.Save
{
    public static class NetworkQuestSaveSystem
    {
        public static void Save(string playerId, QuestSaveData data)
        {
            string path = GetPath(playerId);

            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(path, json);
        }

        public static QuestSaveData Load(string playerId)
        {
            string path = GetPath(playerId);

            if (!File.Exists(path))
                return new QuestSaveData();

            string json = File.ReadAllText(path);

            return JsonUtility.FromJson<QuestSaveData>(json);
        }

        private static string GetPath(string playerId)
        {
            return Path.Combine(Application.persistentDataPath,
                $"quests_{playerId}.json");
        }
    }
}