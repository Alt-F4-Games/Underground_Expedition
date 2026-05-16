namespace Network.Quests.Interfaces
{
    public interface IQuestObjective
    {
        void Initialize();
        void Dispose();

        bool IsCompleted();
        float GetProgress();
    }
    
    public interface IQuestRequirement
    {
        bool IsMet();
    }
}