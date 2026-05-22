using Fusion;
using Network.Interaction;
using Network.Inventory;
using Network.Quests.Definitions;
using Network.Quests.Enums;
using Network.Quests.Runtime;
using Network.Quests.Services;
using UnityEngine;

namespace Network.Quests.World
{
    public class QuestNpc : InteractableBase
    {
        [Header("Quest Database")]
        [SerializeField]
        private QuestDatabase questDatabase;

        [Header("Rewards")]
        [SerializeField]
        private NetworkInventorySystem inventory;

        public override void OnInteract(
            NetworkPlayerController player)
        {
            if (!HasStateAuthority)
                return;

            if (questDatabase == null)
                return;

            QuestManager manager =
                QuestManager.Instance;

            foreach (var quest in questDatabase.GetAllQuests())
            {
                if (quest == null)
                    continue;

                HandleQuest(
                    manager,
                    quest);
            }
        }

        // =====================================================
        // QUEST FLOW
        // =====================================================

        private void HandleQuest(
            QuestManager manager,
            QuestDefinitionSO quest)
        {
            // =================================================
            // ALREADY COMPLETED
            // =================================================

            if (manager.IsQuestCompleted(
                    quest.questId))
            {
                return;
            }

            // =================================================
            // ACTIVE QUEST
            // =================================================

            if (manager.TryGetQuest(
                    quest.questId,
                    out QuestRuntime runtime))
            {
                HandleExistingQuest(
                    manager,
                    runtime,
                    quest);

                return;
            }

            // =================================================
            // ACCEPT QUEST
            // =================================================

            bool canAccept =
                QuestValidationService
                    .CanAcceptQuest(
                        quest,
                        manager);

            if (!canAccept)
                return;

            var acceptedRuntime =
                QuestService.AcceptQuest(
                    quest,
                    manager);

            if (acceptedRuntime == null)
                return;

            Debug.Log(
                $"[QuestNpc] Quest accepted: {quest.questId}");

            // IMPORTANTE:
            // aceptar SOLO UNA quest por interacción
            return;
        }

        // =====================================================
        // EXISTING QUEST
        // =====================================================

        private void HandleExistingQuest(
            QuestManager manager,
            QuestRuntime runtime,
            QuestDefinitionSO quest)
        {
            switch (runtime.Status)
            {
                case QuestStatus.InProgress:

                    Debug.Log(
                        $"[QuestNpc] Quest in progress: {quest.questId}");

                    break;

                case QuestStatus.Completed:

                    QuestService.ClaimRewards(
                        runtime,
                        inventory);

                    manager.CompleteQuest(
                        quest.questId);

                    Debug.Log(
                        $"[QuestNpc] Quest completed: {quest.questId}");

                    break;
            }
        }
    }
}