using System.Collections.Generic;

namespace Network.Quests.Runtime
{
    public class QuestProgressData
    {
        public Dictionary<int, QuestObjectiveProgress>
            Objectives = new();
    }
}