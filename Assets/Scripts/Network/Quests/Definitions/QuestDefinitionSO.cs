// =====================================================
// QuestDefinitionSO.cs
// =====================================================

using System;
using System.Collections.Generic;
using Network.Quests.Enums;
using UnityEngine;

namespace Network.Quests.Definitions
{
    [CreateAssetMenu(menuName = "Quests/Quest Definition")]
    public class QuestDefinitionSO : ScriptableObject
    {
        [Header("Identification")]
        public string questId;

        public string questName;

        [TextArea]
        public string description;

        [Header("Type")]
        public QuestType questType;

        [Header("Requirements")]
        public QuestRequirementType requirementType;

        public string requiredQuestId;

        [Header("Objectives")]
        public List<QuestObjectiveDefinition> objectives = new();

        [Header("Rewards")]
        public List<RewardDefinition> rewards = new();
    }

    [Serializable]
    public class RewardDefinition
    {
        public string itemId;

        public int quantity;

        public int experience;
    }

    [Serializable]
    public class QuestObjectiveDefinition
    {
        public string objectiveId;

        public string displayName;

        public QuestObjectiveType objectiveType;

        public string targetId;

        public int requiredAmount = 1;
    }
}