using System;
using System.Collections.Generic;
using Network.Quests.Enums;
using UnityEngine;

namespace Network.Quests.Definitions
{
    // =====================================================
    // REWARDS
    // =====================================================

    [Serializable]
    public class RewardDefinition
    {
        [Header("Item Reward")]
        public string itemId;

        public int quantity;

        [Header("Optional XP Reward")]
        public int experience;
    }

    // =====================================================
    // REQUIREMENTS
    // =====================================================

    [Serializable]
    public class QuestRequirementDefinition
    {
        [Header("Quest Dependency")]
        public string requiredQuestId;

        [Header("Player Requirement")]
        public int requiredLevel;
    }

    // =====================================================
    // OBJECTIVES
    // =====================================================

    [Serializable]
    public class QuestObjectiveDefinition
    {
        [Header("Type")]
        public QuestObjectiveType questObjectiveType;

        [TextArea]
        public string description;

        // =================================================
        // TARGETS
        // =================================================

        [Header("Target")]
        public string targetId;

        /*
            Examples:

            KillEnemy      -> slime
            CollectItem    -> wood_log
            CraftItem      -> iron_sword
            ExploreArea    -> ancient_ruins
            Interact       -> altar_01
        */

        // =================================================
        // AMOUNT
        // =================================================

        [Header("Amount")]
        public int requiredAmount = 1;
    }

    // =====================================================
    // STEPS
    // =====================================================

    [Serializable]
    public class QuestStepDefinition
    {
        public string stepId;

        public string stepName;

        [TextArea]
        public string description;

        public QuestRequirementDefinition requirement;

        public List<QuestObjectiveDefinition> objectives = new();
    }
}