// =====================================================
// QuestEnums.cs
// =====================================================

namespace Network.Quests.Enums
{
    public enum QuestType
    {
        Main,
        Secondary
    }

    public enum QuestStatus
    {
        Locked,
        Available,
        InProgress,
        Completed,
        RewardClaimed
    }

    public enum QuestObjectiveType
    {
        KillEnemy,
        CollectItem,
        CraftItem,
        Interact,
        ExploreArea
    }

    public enum QuestRequirementType
    {
        None,
        RequireCompletedQuest
    }
}