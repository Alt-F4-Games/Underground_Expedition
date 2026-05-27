using System;
using System.Collections.Generic;

namespace Network.Quests.Runtime
{
    [Serializable]
    public class QuestObjectiveState
    {
        public int CurrentAmount;
    }

    [Serializable]
    public class QuestStepState
    {
        public List<QuestObjectiveState>
            Objectives = new();
    }

    [Serializable]
    public class QuestRuntimeState
    {
        public string QuestId;

        public int CurrentStepIndex;

        public bool IsCompleted;

        public bool RewardClaimed;

        public List<QuestStepState>
            Steps = new();
    }
    
    [Serializable]
    public class QuestSyncData
    {
        public List<QuestRuntimeState>
            ActiveQuests = new();

        public List<string>
            CompletedQuests = new();
    }
}