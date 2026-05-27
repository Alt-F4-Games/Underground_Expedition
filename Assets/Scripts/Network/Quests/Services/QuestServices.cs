using Events;
using Network.Inventory;
using Network.Quests.Definitions;
using Network.Quests.Runtime;
using Tools.EventSystem;
using UnityEngine;

namespace Network.Quests.Services
{
    public static class QuestRewardService
    {
        public static void GiveRewards(
            NetworkInventorySystem inventory,
            QuestDefinitionSO definition)
        {
            if (inventory == null || definition == null)
                return;

            foreach (var reward in definition.rewards)
            {
                int networkId =
                    ItemDatabase.Instance.GetNetworkId(
                        reward.itemId);

                inventory.Server_AddItemGlobal(
                    networkId,
                    reward.quantity);

                if (reward.experience > 0)
                {
                    Debug.Log(
                        $"Give EXP: {reward.experience}");
                }
            }
        }
    }
    
    public static class QuestRequirementService
    {
        public static bool CanStartQuest(
            QuestDefinitionSO definition,
            NetworkQuestManager manager)
        {
            if (definition == null)
                return false;

            foreach (var step in definition.steps)
            {
                if (step.requirement == null)
                    continue;

                if (!ValidateRequirement(step.requirement, manager))
                    return false;
            }

            return true;
        }

        private static bool ValidateRequirement(
            QuestRequirementDefinition requirement,
            NetworkQuestManager manager)
        {
            if (!string.IsNullOrWhiteSpace(
                    requirement.requiredQuestId))
            {
                bool completed =
                    manager.IsQuestCompleted(
                        requirement.requiredQuestId);

                if (!completed)
                    return false;
            }

            return true;
        }
    }
    
    public static class QuestStepService
    {
        public static void EvaluateStep(
            QuestRuntime quest)
        {
            if (quest == null)
                return;

            var currentStep =
                quest.CurrentStep;

            if (currentStep == null)
                return;

            if (!currentStep.IsCompleted())
                return;

            int completedStep =
                quest.CurrentStepIndex;

            EventController.Instance.TriggerEvent(
                new ObjectiveCompletedEvent
                {
                    quest = quest,
                    StepIndex = completedStep
                });
            
            quest.AdvanceStep();
        }
    }
    
    public static class QuestValidationService
    {
        public static bool CanAcceptQuest(
            QuestDefinitionSO definition,
            NetworkQuestManager manager)
        {
            if (definition == null)
                return false;

            if (manager.HasQuest(
                    definition.questId))
                return false;

            if (manager.IsQuestCompleted(
                    definition.questId))
                return false;

            return true;
        }

        public static bool CanTurnInQuest(
            Runtime.QuestRuntime runtime)
        {
            if (runtime == null)
                return false;

            return runtime.Status ==
                   Enums.QuestStatus.Completed;
        }
    }
    
}    