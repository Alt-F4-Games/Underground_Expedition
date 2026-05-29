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

        // =====================================================
        // TYPE
        // =====================================================

        [Header("Quest Type")]
        public QuestType questType;

        // =====================================================
        // REQUIREMENTS
        // =====================================================

        [Header("Requirements")]
        public QuestRequirementType requirementType;

        public string requiredQuestId;

        // =====================================================
        // SCOPES
        // =====================================================

        [Header("Scopes")]
        public ProgressScope progressScope;

        public CompletionScope completionScope;

        public RewardScope rewardScope;

        // =====================================================
        // NPC
        // =====================================================

        [Header("NPC")]
        public string npcId;

        // =====================================================
        // STEPS
        // =====================================================

        [Header("Steps")]
        public List<QuestStepDefinition> steps = new();

        // =====================================================
        // REWARDS
        // =====================================================

        [Header("Rewards")]
        public List<RewardDefinition> rewards = new();
    }
}