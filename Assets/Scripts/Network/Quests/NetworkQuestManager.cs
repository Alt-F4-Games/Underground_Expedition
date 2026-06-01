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

        private readonly Dictionary<string, QuestRuntime> _activeQuests = new();

        public IReadOnlyDictionary<string, QuestRuntime> ActiveQuests => _activeQuests;

        private readonly HashSet<string> _completedQuests = new();

        private NetworkQuestSession Session => NetworkQuestSession.Instance;

        private string LocalPlayerId => SystemInfo.deviceUniqueIdentifier;

        private int _lastAcceptedMainQuestCount;

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                Local = this;
            }

            SyncSharedMainQuests();

            if (Session != null)
            {
                _lastAcceptedMainQuestCount =
                    Session.AcceptedMainQuests.Count;
            }
        }

        public override void FixedUpdateNetwork()
        {
            SyncAcceptedMainQuests();
            SyncMainQuestProgress();
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

        private void SyncSharedMainQuests()
        {
            if (Session == null)
                return;

            foreach (var pair in Session.AcceptedMainQuests)
            {
                string questId =
                    pair.Key.ToString();

                QuestDefinitionSO definition =
                    database.GetQuestById(
                        questId);

                if (definition == null)
                    continue;

                AddQuestLocally(
                    definition);
            }
        }

        private void SyncAcceptedMainQuests()
        {
            if (Session == null)
                return;

            int currentCount =
                Session.AcceptedMainQuests.Count;

            if (currentCount ==
                _lastAcceptedMainQuestCount)
            {
                return;
            }

            _lastAcceptedMainQuestCount =
                currentCount;

            SyncSharedMainQuests();
        }

        private void SyncMainQuestProgress()
        {
            if (Session == null)
                return;

            foreach (var runtime
                     in _activeQuests.Values)
            {
                if (runtime.Definition.questType !=
                    QuestType.Main)
                {
                    continue;
                }

                for (int i = 0;
                     i < runtime.State.objectives.Count;
                     i++)
                {
                    runtime.State.objectives[i]
                        .currentAmount =
                        Session.GetObjectiveProgress(
                            runtime.QuestId,
                            i);
                }

                if (Session.IsMainQuestCompleted(
                        runtime.QuestId))
                {
                    runtime.State.isCompleted = true;
                }
            }
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

            if (Session != null &&
                Session.IsMainQuestCompleted(
                    questId))
            {
                return true;
            }

            return false;
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
            QuestDefinitionSO definition =
                database.GetQuestById(
                    questId);

            if (definition == null)
                return;

            if (definition.questType ==
                QuestType.Main)
            {
                AcceptMainQuestShared(
                    definition);

                return;
            }

            if (_activeQuests.ContainsKey(
                    questId))
            {
                return;
            }

            AddQuestLocally(
                definition);
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

                if (runtime.Definition.questType ==
                    QuestType.Main)
                {
                    int current =
                        Session.GetObjectiveProgress(
                            runtime.QuestId,
                            i);

                    current += amount;

                    current = Mathf.Min(
                        current,
                        objectiveDefinition.requiredAmount);

                    Session.SetObjectiveProgress(
                        runtime.QuestId,
                        i,
                        current);
                }
                else
                {
                    var state =
                        runtime.State.objectives[i];

                    state.currentAmount += amount;

                    state.currentAmount =
                        Mathf.Min(
                            state.currentAmount,
                            objectiveDefinition.requiredAmount);
                }
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
                int current;

                if (runtime.Definition.questType ==
                    QuestType.Main)
                {
                    current =
                        Session.GetObjectiveProgress(
                            runtime.QuestId,
                            i);
                }
                else
                {
                    current =
                        runtime.State.objectives[i]
                            .currentAmount;
                }

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
                Session?.MarkMainQuestCompleted(
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

        private void AcceptMainQuestShared(
            QuestDefinitionSO definition)
        {
            if (Session == null)
                return;

            Session.MarkMainQuestAccepted(
                definition.questId);

            AddQuestLocally(
                definition);
        }

        private void AddQuestLocally(
            QuestDefinitionSO definition)
        {
            if (_activeQuests.ContainsKey(
                    definition.questId))
            {
                return;
            }

            QuestRuntime runtime =
                new QuestRuntime(
                    definition);

            if (Session != null)
            {
                for (int i = 0;
                     i < runtime.State.objectives.Count;
                     i++)
                {
                    runtime.State.objectives[i]
                        .currentAmount =
                        Session.GetObjectiveProgress(
                            definition.questId,
                            i);
                }

                if (Session.IsMainQuestCompleted(
                        definition.questId))
                {
                    runtime.State.isCompleted = true;
                }
            }

            _activeQuests.Add(
                definition.questId,
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
    }
}