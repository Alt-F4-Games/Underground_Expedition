using Network.Inventory;
using Network.Quests.Definitions;
using Network.Quests.Enums;
using Network.Quests.Runtime;

namespace Network.Quests.Services
{
    public static class QuestService
    {
        public static QuestRuntime AcceptQuest(
            QuestDefinitionSO definition,
            QuestManager manager)
        {
            if (definition == null)
                return null;

            if (manager.HasQuest(definition.questId))
                return null;

            bool canStart =
                QuestRequirementService
                    .CanStartQuest(
                        definition,
                        manager);

            if (!canStart)
                return null;

            QuestRuntime runtime =
                new QuestRuntime(definition);

            runtime.SetStatus(
                QuestStatus.Accepted);

            runtime.StartQuest();

            manager.RegisterQuest(runtime);

            return runtime;
        }

        public static void CancelQuest(
            QuestRuntime runtime)
        {
            if (runtime == null)
                return;

            runtime.Dispose();

            runtime.SetStatus(
                QuestStatus.Cancelled);
        }

        public static void ClaimRewards(
            QuestRuntime runtime,
            NetworkInventorySystem inventory)
        {
            if (runtime == null)
                return;

            if (runtime.Status !=
                QuestStatus.Completed)
                return;

            QuestRewardService.GiveRewards(
                inventory,
                runtime.Definition);
            
            runtime.State.RewardClaimed = true;

            runtime.SetStatus(
                QuestStatus.RewardClaimed);
        }
    }
}