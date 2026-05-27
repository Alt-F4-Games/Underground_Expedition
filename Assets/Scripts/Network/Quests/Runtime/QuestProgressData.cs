using System;

namespace Network.Quests.Runtime
{
    [Serializable]
    public struct QuestProgressData
    {
        public string QuestId;

        public int StepIndex;

        public int ObjectiveIndex;

        public int CurrentAmount;
    }
}