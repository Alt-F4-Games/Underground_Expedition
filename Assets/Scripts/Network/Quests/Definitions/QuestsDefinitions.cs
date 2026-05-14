using System;
using Network.Quests.Enums;
using UnityEngine;

namespace Network.Quests.Definitions
{
    [Serializable]
    public class RewardDefinition
    {
        public int itemId;
        public int quantity;

        [Header("Optional XP Reward")]
        public int experience;
    }
    
    [Serializable]
    public class QuestRequirementDefinition
    {
        [Header("Quest Dependency")]
        public string requiredQuestId;

        [Header("Player Requirement")]
        public int requiredLevel;
    }
    
    [Serializable]
    public class QuestObjectiveDefinition
    {
        public ObjectiveType objectiveType;

        [TextArea]
        public string description;

        [Header("Target")]
        public int targetId;

        [Header("Amount")]
        public int requiredAmount = 1;
    }
    
    [Serializable]
    public class QuestStepDefinition
    {
        public QuestRequirementDefinition requirement;
        public QuestObjectiveDefinition objective;
    }
}