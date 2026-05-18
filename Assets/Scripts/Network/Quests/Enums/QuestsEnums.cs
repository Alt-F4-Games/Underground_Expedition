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
        Accepted,
        InProgress,
        Completed,
        RewardPending,
        RewardClaimed,
        Cancelled
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
}