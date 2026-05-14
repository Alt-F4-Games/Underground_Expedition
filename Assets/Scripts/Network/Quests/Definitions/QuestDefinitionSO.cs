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

        [Header("Quest Type")]
        public QuestType questType;

        [Header("Quest Rules")]
        public bool canCancel = true;
        public bool requiresManualAccept = true;
        public bool requiresNpcToComplete = true;

        [Header("Scopes")]
        public ProgressScope progressScope;
        public CompletionScope completionScope;
        public RewardScope rewardScope;

        [Header("NPC")]
        public string npcId;

        [Header("Steps")]
        public List<QuestStepDefinition> steps = new();

        [Header("Rewards")]
        public List<RewardDefinition> rewards = new();
    }
}