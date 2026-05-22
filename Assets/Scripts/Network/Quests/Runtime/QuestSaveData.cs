using System;
using System.Collections.Generic;

namespace Network.Quests.Runtime
{
    [Serializable]
    public class QuestSaveData
    {
        public List<QuestRuntimeState>
            ActiveQuests = new();

        public List<string>
            CompletedQuests = new();
    }
}