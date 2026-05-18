using Network.Inventory;

namespace Network.Quests.Runtime
{
    public static class QuestRewardService
    {
        public static void GiveRewards(QuestRuntime runtime)
        {
            foreach (var reward in runtime.Definition.rewards)
            {
                int networkId = ItemDatabase.Instance.GetNetworkId(reward.itemId);

                // Más adelante:
                // inventory.Server_AddItemGlobal(...)
            }
        }
    }
    
    public static class QuestRequirementService
    {
        public static bool CanStartQuest(
            string questId)
        {
            return true;
        }
    }
    
    public static class QuestStepService
    {
        public static void EvaluateStep(
            QuestRuntime runtime)
        {
            if (!runtime.CurrentStep.IsCompleted())
                return;

            runtime.AdvanceStep();
        }
    }
    
}    