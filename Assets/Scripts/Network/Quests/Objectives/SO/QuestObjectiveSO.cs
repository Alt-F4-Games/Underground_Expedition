using UnityEngine;

namespace Network.Quests.Objectives.SO
{
    public abstract class QuestObjectiveSO : ScriptableObject
    {
        [Header("Base")]
        public string objectiveId;

        [TextArea]
        public string description;

        public QuestObjectiveType objectiveType;

        [Header("Progress")]
        public int requiredAmount = 1;
    }
}