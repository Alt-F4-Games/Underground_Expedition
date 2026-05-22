using System.Collections.Generic;
using Network.Quests.Definitions;
using UnityEngine;

namespace Network.Quests.Runtime
{
    public static class QuestSaveService
    {
        // =====================================================
        // SAVE
        // =====================================================

        public static QuestSaveData CreateSaveData(
            QuestManager manager)
        {
            QuestSaveData saveData =
                new QuestSaveData();

            // Active quests
            foreach (var runtime
                     in manager.ActiveQuests)
            {
                saveData.ActiveQuests.Add(
                    runtime.State);
            }

            // Completed quests
            saveData.CompletedQuests.AddRange(
                manager.CompletedQuestIds);

            return saveData;
        }

        // =====================================================
        // LOAD
        // =====================================================

        public static void LoadSaveData(
            QuestManager manager,
            QuestSaveData saveData)
        {
            if (manager == null ||
                saveData == null)
                return;

            manager.ClearAll();

            // Restore completed
            foreach (var questId
                     in saveData.CompletedQuests)
            {
                manager.MarkQuestCompleted(
                    questId);
            }

            // Restore active quests
            foreach (var state
                     in saveData.ActiveQuests)
            {
                RestoreQuest(
                    manager,
                    state);
            }
        }

        // =====================================================
        // RESTORE
        // =====================================================

        private static void RestoreQuest(
            QuestManager manager,
            QuestRuntimeState state)
        {
            if (state == null)
                return;

            QuestDefinitionSO definition =
                QuestDatabase.Instance
                    .GetQuestById(
                        state.QuestId);

            if (definition == null)
            {
                Debug.LogWarning(
                    $"[QuestSaveService] Missing quest definition: {state.QuestId}");

                return;
            }

            QuestRuntime runtime =
                new QuestRuntime(
                    definition,
                    state);

            runtime.StartQuest();

            manager.RegisterQuest(runtime);

            Debug.Log(
                $"[QuestSaveService] Restored quest: {state.QuestId}");
        }
    }
}