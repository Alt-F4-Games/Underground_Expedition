using System;
using System.Collections.Generic;

namespace Network.Quests.Runtime
{
    [Serializable]
    public class QuestObjectiveState
    {
        public int currentAmount;
    }

    [Serializable]
    public class QuestStepState
    {
        public List<QuestObjectiveState> objectives = new();
    }

    [Serializable]
    public class QuestState
    {
        public string questId;

        public int currentStepIndex;

        public bool isCompleted;

        // =====================================================
        // GLOBAL QUEST STATE
        // =====================================================

        public List<QuestStepState> steps = new();

        // =====================================================
        // INDIVIDUAL REWARD CLAIMS
        // =====================================================

        public List<string> claimedPlayerIds = new();
    }

    [Serializable]
    public class QuestSaveData
    {
        public List<QuestState> active = new();
    }
}