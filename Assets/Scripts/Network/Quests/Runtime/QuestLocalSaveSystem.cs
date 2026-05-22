using System.IO;
using UnityEngine;

namespace Network.Quests.Runtime
{
    public static class QuestLocalSaveSystem
    {
        private const string FileName =
            "quests_save.json";

        private static string SavePath =>
            Path.Combine(
                Application.persistentDataPath,
                FileName);

        // =====================================================
        // SAVE
        // =====================================================

        public static void Save(
            QuestManager manager)
        {
            if (manager == null)
                return;

            QuestSaveData saveData =
                QuestSaveService.CreateSaveData(
                    manager);

            string json =
                JsonUtility.ToJson(
                    saveData,
                    true);

            File.WriteAllText(
                SavePath,
                json);

            Debug.Log(
                $"[QuestSave] Saved quests -> {SavePath}");
        }

        // =====================================================
        // LOAD
        // =====================================================

        public static void Load(
            QuestManager manager)
        {
            if (manager == null)
                return;

            if (!File.Exists(SavePath))
            {
                Debug.Log(
                    "[QuestSave] No save file found.");

                return;
            }

            string json =
                File.ReadAllText(
                    SavePath);

            QuestSaveData saveData =
                JsonUtility.FromJson<QuestSaveData>(
                    json);

            QuestSaveService.LoadSaveData(
                manager,
                saveData);

            Debug.Log(
                "[QuestSave] Quests loaded.");
        }

        // =====================================================
        // DELETE
        // =====================================================

        public static void DeleteSave()
        {
            if (!File.Exists(SavePath))
                return;

            File.Delete(
                SavePath);

            Debug.Log(
                "[QuestSave] Save deleted.");
        }
    }
}