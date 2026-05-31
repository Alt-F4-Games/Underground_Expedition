// =====================================================
// QuestState.cs
// =====================================================

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
    public class QuestState
    {
        public string questId;

        public bool isCompleted;

        public List<QuestObjectiveState> objectives = new();

        public List<string> claimedPlayerIds = new();
    }

    [Serializable]
    public class QuestSaveData
    {
        public List<QuestState> active = new();
    }
}