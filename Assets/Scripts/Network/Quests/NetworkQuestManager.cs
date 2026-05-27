using System.Collections.Generic;
using Events;
using Fusion;
using Network.Inventory;
using Network.Quests.Definitions;
using Network.Quests.Enums;
using Network.Quests.Runtime;
using Network.Quests.Save;
using Network.Quests.Services;
using Tools.EventSystem;
using UnityEngine;

namespace Network.Quests
{
    public class NetworkQuestManager : NetworkBehaviour
    {
        // =====================================================
        // STATIC
        // =====================================================

        public static NetworkQuestManager Local { get; private set; }

        public static readonly List<NetworkQuestManager> AllManagers = new();

        // =====================================================
        // RUNTIME DATA
        // =====================================================

        private readonly Dictionary<string, QuestRuntime> _runtimeCache = new();

        public IReadOnlyCollection<QuestRuntime> ActiveQuests => _runtimeCache.Values;

        public HashSet<string> CompletedQuestIds = new();

        // =====================================================
        // UNITY
        // =====================================================

        public override void Spawned()
        {
            AllManagers.Add(this);

            if (Object.HasInputAuthority)
            {
                Local = this;

                NetworkQuestSaveSystem.Load(
                    Object.InputAuthority,
                    this);
            }

            if (HasStateAuthority)
            {
                SendFullSync();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            AllManagers.Remove(this);

            if (Local == this)
            {
                Local = null;
            }
        }

        // =====================================================
        // ACCESS
        // =====================================================

        public bool HasQuest(string questId)
        {
            return _runtimeCache.ContainsKey(
                questId);
        }

        public bool TryGetQuest(string questId, out QuestRuntime runtime)
        {
            return _runtimeCache.TryGetValue(
                questId,
                out runtime);
        }

        public bool IsQuestCompleted(string questId)
        {
            return CompletedQuestIds.Contains(
                questId);
        }

        public void RegisterQuest(QuestRuntime runtime)
        {
            if (runtime == null)
                return;

            string questId =
                runtime.Definition.questId;

            if (_runtimeCache.ContainsKey(
                    questId))
            {
                return;
            }

            _runtimeCache.Add(
                questId,
                runtime);
        }

        public void CompleteQuest(string questId)
        {
            if (!_runtimeCache.TryGetValue(
                    questId,
                    out QuestRuntime runtime))
            {
                return;
            }

            runtime.Dispose();

            _runtimeCache.Remove(
                questId);

            CompletedQuestIds.Add(
                questId);
        }

        // =====================================================
        // ACCEPT QUEST
        // =====================================================

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_AcceptQuest(string questId)
        {
            QuestDefinitionSO definition =
                QuestDatabase.Instance
                    .GetQuestById(questId);

            if (definition == null)
                return;

            if (HasQuest(questId))
                return;

            QuestRuntime runtime =
                new QuestRuntime(definition);

            runtime.StartQuest();

            RegisterQuest(runtime);

            RPC_SyncAcceptQuest(
                questId);
        }

        // =====================================================
        // CLAIM REWARD
        // =====================================================

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_ClaimReward(string questId)
        {
            if (!_runtimeCache.TryGetValue(
                    questId,
                    out QuestRuntime runtime))
            {
                return;
            }

            if (runtime.Status !=
                QuestStatus.Completed)
            {
                return;
            }

            NetworkInventorySystem inventory =
                GetComponent<NetworkInventorySystem>();

            QuestService.ClaimRewards(
                runtime,
                inventory);

            CompleteQuest(questId);

            RPC_SyncQuestCompleted(
                questId);
        }

        // =====================================================
        // OBJECTIVE PROGRESS
        // =====================================================

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RPC_AddProgressByTarget(QuestObjectiveType type, string targetId, int amount)
        {
            foreach (QuestRuntime runtime
                     in _runtimeCache.Values)
            {
                if (runtime.CurrentStep == null)
                    continue;

                int objectiveIndex =
                    FindObjectiveIndex(
                        runtime,
                        type,
                        targetId);

                if (objectiveIndex < 0)
                    continue;

                ApplyProgress(
                    runtime,
                    runtime.QuestId,
                    runtime.CurrentStepIndex,
                    objectiveIndex,
                    amount);
            }
        }

        private int FindObjectiveIndex(QuestRuntime runtime, QuestObjectiveType type, string targetId)
        {
            if (runtime.CurrentStep == null)
                return -1;

            for (int i = 0;
                 i < runtime.CurrentStep
                     .ObjectiveRuntimes.Count;
                 i++)
            {
                var objective =
                    runtime.CurrentStep
                        .ObjectiveRuntimes[i];

                if (objective.Definition
                        .questObjectiveType != type)
                {
                    continue;
                }

                if (objective.Definition
                        .targetId != targetId)
                {
                    continue;
                }

                return i;
            }

            return -1;
        }

        private void ApplyProgress(QuestRuntime runtime, string questId, int stepIndex, int objectiveIndex, int amount)
        {
            QuestObjectiveState objectiveState =
                runtime.State
                    .Steps[stepIndex]
                    .Objectives[objectiveIndex];

            QuestObjectiveDefinition definition =
                runtime.CurrentStep
                    .Definition
                    .objectives[objectiveIndex];

            objectiveState.CurrentAmount += amount;

            if (objectiveState.CurrentAmount >
                definition.requiredAmount)
            {
                objectiveState.CurrentAmount =
                    definition.requiredAmount;
            }

            RPC_SyncObjectiveProgress(
                questId,
                stepIndex,
                objectiveIndex,
                objectiveState.CurrentAmount);

            EvaluateQuestCompletion(
                runtime);
        }

        private void EvaluateQuestCompletion(QuestRuntime runtime)
        {
            if (runtime == null)
                return;

            if (runtime.CurrentStep == null)
                return;

            if (!runtime.CurrentStep
                    .IsCompleted())
            {
                return;
            }

            runtime.AdvanceStep();

            if (runtime.Status ==
                QuestStatus.Completed)
            {
                RPC_SyncQuestFinished(
                    runtime.QuestId);
            }
        }

        // =====================================================
        // CLIENT SYNC
        // =====================================================

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SyncAcceptQuest(string questId)
        {
            if (HasQuest(questId))
                return;

            QuestDefinitionSO definition =
                QuestDatabase.Instance
                    .GetQuestById(questId);

            if (definition == null)
                return;

            QuestRuntime runtime =
                new QuestRuntime(definition);

            runtime.StartQuest();

            RegisterQuest(runtime);

            RefreshUI();

            SaveProgress();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SyncObjectiveProgress(string questId, int stepIndex, int objectiveIndex, int currentAmount)
        {
            if (!_runtimeCache.TryGetValue(
                    questId,
                    out QuestRuntime runtime))
            {
                return;
            }

            runtime.State
                    .Steps[stepIndex]
                    .Objectives[objectiveIndex]
                    .CurrentAmount =
                currentAmount;

            RefreshUI();

            SaveProgress();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SyncQuestFinished(string questId)
        {
            if (!_runtimeCache.TryGetValue(
                    questId,
                    out QuestRuntime runtime))
            {
                return;
            }

            runtime.SetStatus(
                QuestStatus.Completed);

            RefreshUI();

            SaveProgress();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SyncQuestCompleted(string questId)
        {
            CompleteQuest(
                questId);

            RefreshUI();

            SaveProgress();
        }

        // =====================================================
        // FULL SYNC
        // =====================================================

        private void SendFullSync()
        {
            QuestSyncData data =
                CreateSyncData();

            string json =
                JsonUtility.ToJson(data);

            RPC_FullSync(json);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        public void RPC_FullSync(string json)
        {
            QuestSyncData data =
                JsonUtility.FromJson<QuestSyncData>(
                    json);

            if (data == null)
                return;

            LoadFromSyncData(data);
        }

        public QuestSyncData CreateSyncData()
        {
            QuestSyncData data =
                new QuestSyncData();

            foreach (QuestRuntime runtime
                     in _runtimeCache.Values)
            {
                data.ActiveQuests.Add(
                    runtime.State);
            }

            data.CompletedQuests.AddRange(
                CompletedQuestIds);

            return data;
        }

        public void LoadFromSyncData(QuestSyncData data)
        {
            _runtimeCache.Clear();

            CompletedQuestIds.Clear();

            foreach (string completedQuest
                     in data.CompletedQuests)
            {
                CompletedQuestIds.Add(
                    completedQuest);
            }

            foreach (QuestRuntimeState state
                     in data.ActiveQuests)
            {
                QuestDefinitionSO definition =
                    QuestDatabase.Instance
                        .GetQuestById(
                            state.QuestId);

                if (definition == null)
                    continue;

                QuestRuntime runtime =
                    new QuestRuntime(
                        definition,
                        state);

                runtime.StartQuest();

                _runtimeCache.Add(
                    state.QuestId,
                    runtime);
            }

            RefreshUI();
        }

        // =====================================================
        // SAVE
        // =====================================================

        private void SaveProgress()
        {
            if (!Object.HasInputAuthority)
                return;

            NetworkQuestSaveSystem.Save(
                Object.InputAuthority,
                this);
        }

        // =====================================================
        // UI
        // =====================================================

        private void RefreshUI()
        {
            EventController.Instance.TriggerEvent(
                new QuestUIRefreshEvent());
        }

        // =====================================================
        // CLEANUP
        // =====================================================

        public void ClearAll()
        {
            foreach (QuestRuntime runtime
                     in _runtimeCache.Values)
            {
                runtime.Dispose();
            }

            _runtimeCache.Clear();

            CompletedQuestIds.Clear();
        }
    }
}