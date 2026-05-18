using System.Collections.Generic;

namespace Network.Quests.Runtime
{
    public class QuestObjectiveProgress
    {
        public int CurrentAmount;

        public int RequiredAmount;

        public bool IsCompleted =>
            CurrentAmount >= RequiredAmount;
    }
    
    public class QuestProgressData
    {
        public Dictionary<int, QuestObjectiveProgress>
            Objectives = new();
    }
}