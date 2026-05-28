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
    
    public enum ProgressScope
    {
        Individual,
        Shared
    }
    
    public enum CompletionScope
    {
        Individual,
        Shared
    }
    
    public enum RewardScope
    {
        Individual,
        Shared
    }
    
    public enum QuestRequirementType
    {
        None,
        RequireCompletedAndClaimedQuest
    }
}