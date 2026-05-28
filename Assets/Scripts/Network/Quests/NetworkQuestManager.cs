using System.Collections.Generic;
using Events;
using Fusion;
using Local.Progression;
using Network.Inventory;
using Network.Quests.Definitions;
using Network.Quests.Enums;
using Network.Quests.Runtime;
using Tools.EventSystem;
using UnityEngine;

namespace Network.Quests
{
    public class NetworkQuestManager : NetworkBehaviour
    {
        public static NetworkQuestManager Local { get; private set; }

        [Header("Database")]
        [SerializeField]
        private QuestDatabase database;

        private readonly Dictionary<string, QuestRuntime>
            _activeQuests = new();

        public IReadOnlyDictionary<string, QuestRuntime>
            ActiveQuests => _activeQuests;

        private string LocalPlayerId =>
            SystemInfo.deviceUniqueIdentifier;

        // =====================================================
        // UNITY
        // =====================================================

        private void Awake()
        {
            if (Local != null &&
                Local != this)
            {
                Destroy(gameObject);
                return;
            }

            Local = this;
        }

        // =====================================================
        // HELPERS
        // =====================================================

        public bool TryGetQuest(
            string questId,
            out QuestRuntime runtime)
        {
            return _activeQuests
                .TryGetValue(
                    questId,
                    out runtime);
        }

        public bool IsQuestRewardClaimed(
            string questId)
        {
            if (!_activeQuests.TryGetValue(
                    questId,
                    out QuestRuntime runtime))
                return false;

            return runtime.HasPlayerClaimed(
                LocalPlayerId);
        }

        public bool CanAcceptQuest(
            QuestDefinitionSO definition)
        {
            if (definition == null)
                return false;

            if (_activeQuests.ContainsKey(
                    definition.questId))
                return false;

            foreach (var step in definition.steps)
            {
                if (step.requirement == null)
                    continue;

                if (step.requirement.requirementType ==
                    QuestRequirementType.None)
                    continue;

                if (step.requirement.requirementType ==
                    QuestRequirementType
                        .RequireCompletedAndClaimedQuest)
                {
                    if (!_activeQuests.TryGetValue(
                            step.requirement.requiredQuestId,
                            out QuestRuntime requiredRuntime))
                    {
                        return false;
                    }

                    if (!requiredRuntime.State.isCompleted)
                        return false;

                    if (!requiredRuntime.HasPlayerClaimed(
                            LocalPlayerId))
                        return false;
                }
            }

            return true;
        }

        public bool IsQuestLocked(
            QuestDefinitionSO definition)
        {
            return !CanAcceptQuest(definition);
        }

        // =====================================================
        // ACCEPT QUEST
        // =====================================================

        [Rpc(RpcSources.InputAuthority,
            RpcTargets.StateAuthority)]
        public void RPC_AcceptQuest(
            string questId)
        {
            if (_activeQuests.ContainsKey(
                    questId))
                return;

            QuestDefinitionSO definition =
                database.GetQuestById(
                    questId);

            if (definition == null)
                return;

            QuestRuntime runtime =
                new QuestRuntime(
                    definition);

            _activeQuests.Add(
                questId,
                runtime);

            EventController.Instance
                .TriggerEvent(
                    new QuestAcceptedEvent
                    {
                        quest = runtime
                    });

            EventController.Instance
                .TriggerEvent(
                    new QuestUIRefreshEvent());
        }

        // =====================================================
        // CLAIM REWARD
        // =====================================================

        [Rpc(RpcSources.InputAuthority,
            RpcTargets.StateAuthority)]
        public void RPC_ClaimReward(
            string questId)
        {
            if (!_activeQuests.TryGetValue(
                    questId,
                    out QuestRuntime runtime))
                return;

            if (!runtime.State.isCompleted)
                return;

            string playerId =
                SystemInfo.deviceUniqueIdentifier;

            if (runtime.HasPlayerClaimed(
                    playerId))
                return;

            // =================================================
            // GIVE REWARDS
            // =================================================

            GiveRewards(runtime);

            runtime.MarkRewardClaimed(
                playerId);

            EventController.Instance
                .TriggerEvent(
                    new RewardClaimedEvent
                    {
                        quest = runtime
                    });

            EventController.Instance
                .TriggerEvent(
                    new QuestUIRefreshEvent());
        }

        private void GiveRewards(
            QuestRuntime runtime)
        {
            var inventory =
                GetComponent<NetworkInventorySystem>();

            var exp =
                GetComponent<NetworkExperienceSystem>();

            foreach (var reward
                     in runtime.Definition.rewards)
            {
                // ITEMS

                if (!string.IsNullOrWhiteSpace(
                        reward.itemId))
                {
                    int itemId =
                        ItemDatabase.Instance
                            .GetNetworkId(
                                reward.itemId);

                    if (itemId > 0)
                    {
                        inventory.Server_AddItemGlobal(
                            itemId,
                            reward.quantity);
                    }
                }

                // XP

                if (reward.experience > 0)
                {
                    exp.RPC_RequestAddXP(
                        reward.experience);
                }
            }
        }

        // =====================================================
        // QUEST EVENTS
        // =====================================================

        [Rpc(RpcSources.InputAuthority,
            RpcTargets.StateAuthority)]
        public void RPC_ReportQuestEvent(
            int objectiveType,
            string targetId,
            int amount)
        {
            foreach (var runtime
                     in _activeQuests.Values)
            {
                ProcessQuestProgress(
                    runtime,
                    (QuestObjectiveType)objectiveType,
                    targetId,
                    amount);
            }

            EventController.Instance
                .TriggerEvent(
                    new QuestUIRefreshEvent());
        }

        private void ProcessQuestProgress(
            QuestRuntime runtime,
            QuestObjectiveType objectiveType,
            string targetId,
            int amount)
        {
            if (runtime.State.isCompleted)
                return;

            int stepIndex =
                runtime.State.currentStepIndex;

            if (stepIndex >=
                runtime.Definition.steps.Count)
                return;

            var stepDefinition =
                runtime.Definition.steps[
                    stepIndex];

            var stepState =
                runtime.State.steps[
                    stepIndex];

            for (int i = 0;
                 i < stepDefinition.objectives.Count;
                 i++)
            {
                var objectiveDefinition =
                    stepDefinition.objectives[i];

                if (objectiveDefinition
                        .questObjectiveType
                    != objectiveType)
                    continue;

                if (objectiveDefinition.targetId
                    != targetId)
                    continue;

                var objectiveState =
                    stepState.objectives[i];

                objectiveState.currentAmount +=
                    amount;

                if (objectiveState.currentAmount >
                    objectiveDefinition.requiredAmount)
                {
                    objectiveState.currentAmount =
                        objectiveDefinition.requiredAmount;
                }
            }

            CheckStepCompletion(runtime);
        }

        // =====================================================
        // COMPLETION
        // =====================================================

        private void CheckStepCompletion(
            QuestRuntime runtime)
        {
            int stepIndex =
                runtime.State.currentStepIndex;

            var stepDefinition =
                runtime.Definition.steps[
                    stepIndex];

            var stepState =
                runtime.State.steps[
                    stepIndex];

            for (int i = 0;
                 i < stepDefinition.objectives.Count;
                 i++)
            {
                int current =
                    stepState.objectives[i]
                        .currentAmount;

                int required =
                    stepDefinition.objectives[i]
                        .requiredAmount;

                if (current < required)
                    return;
            }

            runtime.State.currentStepIndex++;

            if (runtime.State.currentStepIndex >=
                runtime.Definition.steps.Count)
            {
                runtime.State.isCompleted = true;

                EventController.Instance
                    .TriggerEvent(
                        new QuestCompletedEvent
                        {
                            quest = runtime
                        });
            }
        }
    }
}