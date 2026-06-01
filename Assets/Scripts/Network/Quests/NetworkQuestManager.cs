// =====================================================
// NetworkQuestManager.cs
// =====================================================

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
        public static NetworkQuestManager Local
        { get; private set; }

        [Header("Database")]
        [SerializeField]
        private QuestDatabase database;

        private readonly Dictionary<string,
            QuestRuntime>
            _activeQuests = new();

        public IReadOnlyDictionary<string,
            QuestRuntime>
            ActiveQuests => _activeQuests;

        private readonly HashSet<string>
            _completedQuests = new();

        private static readonly SharedQuestState
            SharedState = new();

        private string LocalPlayerId =>
            SystemInfo.deviceUniqueIdentifier;

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                Local = this;
            }
        }

        public override void Despawned(
            NetworkRunner runner,
            bool hasState)
        {
            if (Local == this)
            {
                Local = null;
            }
        }

        public bool TryGetQuest(
            string questId,
            out QuestRuntime runtime)
        {
            return _activeQuests.TryGetValue(
                questId,
                out runtime);
        }

        public bool IsQuestRewardClaimed(
            string questId)
        {
            if (!_activeQuests.TryGetValue(
                    questId,
                    out QuestRuntime runtime))
            {
                return false;
            }

            return runtime.HasPlayerClaimed(
                LocalPlayerId);
        }

        public bool HasCompletedQuest(
            string questId)
        {
            if (_completedQuests.Contains(
                    questId))
            {
                return true;
            }

            return SharedState.IsCompleted(
                questId);
        }

        public bool CanAcceptQuest(
            QuestDefinitionSO definition)
        {
            if (definition == null)
                return false;

            if (_activeQuests.ContainsKey(
                    definition.questId))
            {
                return false;
            }

            if (definition.requirementType ==
                QuestRequirementType
                    .RequireCompletedQuest)
            {
                if (!HasCompletedQuest(
                        definition.requiredQuestId))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsQuestLocked(
            QuestDefinitionSO definition)
        {
            if (definition == null)
                return true;

            if (definition.requirementType ==
                QuestRequirementType.None)
            {
                return false;
            }

            if (definition.requirementType ==
                QuestRequirementType
                    .RequireCompletedQuest)
            {
                return !HasCompletedQuest(
                    definition.requiredQuestId);
            }

            return false;
        }

        [Rpc(
            RpcSources.InputAuthority,
            RpcTargets.StateAuthority)]
        public void RPC_AcceptQuest(
            string questId)
        {
            if (_activeQuests.ContainsKey(
                    questId))
            {
                return;
            }

            QuestDefinitionSO definition =
                database.GetQuestById(
                    questId);

            if (definition == null)
                return;

            QuestRuntime runtime =
                new QuestRuntime(
                    definition);

            if (definition.questType ==
                QuestType.Main)
            {
                if (SharedState.IsCompleted(
                        questId))
                {
                    runtime.State.isCompleted =
                        true;
                }
            }

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

        [Rpc(
            RpcSources.InputAuthority,
            RpcTargets.StateAuthority)]
        public void RPC_ClaimReward(
            string questId)
        {
            if (!_activeQuests.TryGetValue(
                    questId,
                    out QuestRuntime runtime))
            {
                return;
            }

            if (!runtime.State.isCompleted)
                return;

            if (runtime.HasPlayerClaimed(
                    LocalPlayerId))
            {
                return;
            }

            GiveRewards(runtime);

            runtime.MarkRewardClaimed(
                LocalPlayerId);

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

                if (reward.experience > 0)
                {
                    exp.RPC_RequestAddXP(
                        reward.experience);
                }
            }
        }

        [Rpc(
            RpcSources.InputAuthority,
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

            for (int i = 0;
                 i < runtime.Definition.objectives.Count;
                 i++)
            {
                var objectiveDefinition =
                    runtime.Definition.objectives[i];

                if (objectiveDefinition.objectiveType
                    != objectiveType)
                {
                    continue;
                }

                if (objectiveDefinition.targetId
                    != targetId)
                {
                    continue;
                }

                var objectiveState =
                    runtime.State.objectives[i];

                objectiveState.currentAmount +=
                    amount;

                if (objectiveState.currentAmount >
                    objectiveDefinition.requiredAmount)
                {
                    objectiveState.currentAmount =
                        objectiveDefinition.requiredAmount;
                }

                EventController.Instance
                    .TriggerEvent(
                        new QuestObjectiveProgressEvent
                        {
                            quest = runtime,
                            ObjectiveIndex = i,
                            CurrentAmount =
                                objectiveState.currentAmount,
                            RequiredAmount =
                                objectiveDefinition.requiredAmount
                        });
            }

            CheckQuestCompletion(runtime);
        }

        private void CheckQuestCompletion(
            QuestRuntime runtime)
        {
            for (int i = 0;
                 i < runtime.Definition.objectives.Count;
                 i++)
            {
                int current =
                    runtime.State.objectives[i]
                        .currentAmount;

                int required =
                    runtime.Definition.objectives[i]
                        .requiredAmount;

                if (current < required)
                    return;
            }

            runtime.State.isCompleted = true;

            _completedQuests.Add(
                runtime.QuestId);

            if (runtime.Definition.questType ==
                QuestType.Main)
            {
                SharedState.MarkCompleted(
                    runtime.QuestId);
            }

            EventController.Instance
                .TriggerEvent(
                    new QuestCompletedEvent
                    {
                        quest = runtime
                    });

            EventController.Instance
                .TriggerEvent(
                    new QuestUIRefreshEvent());
        }
    }
}