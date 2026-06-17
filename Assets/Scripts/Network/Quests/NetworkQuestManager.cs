using System.Collections.Generic;
using Events;
using Fusion;
using Local.Progression;
using Network.Inventory;
using Network.Items;
using Network.Quests.Definitions;
using Network.Quests.Enums;
using Network.Quests.Runtime;
using Tools.EventSystem;
using UI.Quests;
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
        private PlayerRef LocalPlayerRef => Object.InputAuthority;
        private ChangeDetector _sessionChanges;


        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                Local = this;
            }

            SyncSharedMainQuests();
        }
        

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Local == this)
            {
                Local = null;
            }
        }
        
        public override void Render()
        {
            if (Session == null)
                return;

            _sessionChanges ??= Session.GetChangeDetector(ChangeDetector.Source.SimulationState);

            bool refreshNeeded = false;

            foreach (var change in _sessionChanges.DetectChanges(Session))
            {
                if (change == nameof(NetworkQuestSession .AcceptedMainQuests))
                {
                    SyncSharedMainQuests();
                    refreshNeeded = true;
                }

                if (change == nameof(NetworkQuestSession.ObjectiveProgress))
                {
                    UpdateSharedProgress();
                    refreshNeeded = true;
                }

                if (change == nameof(NetworkQuestSession.CompletedMainQuests))
                {
                    UpdateSharedCompletion();
                    refreshNeeded = true;
                }
                
                if (change == nameof(NetworkQuestSession.ClaimedRewards)) { refreshNeeded = true; }
            }

            if (refreshNeeded)
            {
                EventController.Instance.TriggerEvent(new QuestUIRefreshEvent());
            }
        }
        
        private void UpdateSharedProgress()
        {
            if (Session == null)
                return;

            foreach (var runtime in _activeQuests.Values)
            {
                if (runtime.Definition.questType != QuestType.Main) { continue; }

                for (int i = 0; i < runtime.State.objectives.Count; i++)
                {
                    runtime.State.objectives[i].currentAmount = Session.GetObjectiveProgress(runtime.QuestId, i);
                }
            }
        }
        
        private void UpdateSharedCompletion()
        {
            if (Session == null)
                return;

            foreach (var runtime in _activeQuests.Values)
            {
                if (runtime.Definition.questType != QuestType.Main) { continue; }

                runtime.State.isCompleted = Session.IsMainQuestCompleted( runtime.QuestId);
            }
        }

        public bool TryGetQuest(string questId, out QuestRuntime runtime)
        { return _activeQuests.TryGetValue(questId, out runtime); }

        private void SyncSharedMainQuests()
        {
            if (Session == null)
                return;

            foreach (var pair in Session.AcceptedMainQuests)
            {
                string questId = pair.Key.ToString();

                QuestDefinitionSO definition = database.GetQuestById(questId);

                if (definition == null)
                    continue;

                AddQuestLocally(definition);
            }
        }
        
        private void RefreshSharedQuestData()
        {
            SyncSharedMainQuests();

            if (Session == null)
                return;

            foreach (var runtime in _activeQuests.Values)
            {
                if (runtime.Definition.questType != QuestType.Main) { continue; }

                for (int i = 0; i < runtime.State.objectives.Count; i++)
                {
                    runtime.State.objectives[i].currentAmount = Session.GetObjectiveProgress(runtime.QuestId, i);
                }

                runtime.State.isCompleted = Session.IsMainQuestCompleted(runtime.QuestId);
            }

            EventController.Instance.TriggerEvent(new QuestUIRefreshEvent());
        }

        public bool IsQuestRewardClaimed(string questId)
        {
            if (!Session)
                return false;

            return Session.HasClaimedReward(LocalPlayerRef, questId);
        }

        private bool HasCompletedQuest(string questId)
        {
            if (_completedQuests.Contains(questId)) { return true; }

            if (Session != null && Session.IsMainQuestCompleted(questId)) { return true; }

            return false;
        }

        public bool IsQuestLocked(QuestDefinitionSO definition)
        {
            if (definition == null)
                return true;

            if (definition.requirementType == QuestRequirementType.None)
            { return false; }

            if (definition.requirementType == QuestRequirementType.RequireCompletedQuest)
            { return !HasCompletedQuest(definition.requiredQuestId); }

            return false;
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_AcceptQuest(string questId)
        {
            QuestDefinitionSO definition = database.GetQuestById(questId);

            if (definition == null)
                return;

            if (definition.questType == QuestType.Main)
            {
                AcceptMainQuestShared(definition);
                return;
            }

            if (_activeQuests.ContainsKey(questId)) { return; }

            AddQuestLocally(definition);
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_ClaimReward(string questId)
        {
            PlayerRef playerRef = Object.InputAuthority;
            
            if (!_activeQuests.TryGetValue(questId, out QuestRuntime runtime)) { return; }
            
            if (!Session.IsMainQuestCompleted(questId)) { return; }
            
            if (Session.HasClaimedReward(playerRef, questId)) { return; }
            
            QuestDefinitionSO definition = database.GetQuestById(questId);

            if (definition == null) { return; }
            
            GiveRewards(playerRef, definition);
            
            Session.MarkRewardClaimed(playerRef, questId);
            
            EventController.Instance.TriggerEvent(new QuestUIRefreshEvent());
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_ReportQuestEvent(int objectiveType, string targetId, int amount)
        {
            foreach (var runtime in _activeQuests.Values)
            { ProcessQuestProgress(runtime, (QuestObjectiveType)objectiveType, targetId, amount); }

            EventController.Instance.TriggerEvent(new QuestUIRefreshEvent());
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ShowQuestNotification(int notificationType, string questName)
        {
            QuestNotificationsUI.Instance?.ShowNotification(
                (QuestNotificationType)notificationType,
                questName);
        }

        private void ProcessQuestProgress(QuestRuntime runtime, QuestObjectiveType objectiveType, string targetId, int amount)
        {
            if (runtime.State.isCompleted)
                return;

            for (int i = 0; i < runtime.Definition.objectives.Count; i++)
            {
                var objectiveDefinition = runtime.Definition.objectives[i];

                if (objectiveDefinition.objectiveType != objectiveType) { continue; }

                if (objectiveDefinition.targetId != targetId) { continue; }

                if (runtime.Definition.questType == QuestType.Main)
                {
                    int current = Session.GetObjectiveProgress(runtime.QuestId, i);

                    current += amount;

                    current = Mathf.Min(current, objectiveDefinition.requiredAmount);

                    Session.SetObjectiveProgress(runtime.QuestId, i, current);
                }
                else
                {
                    var state = runtime.State.objectives[i];

                    state.currentAmount += amount;

                    state.currentAmount = Mathf.Min(state.currentAmount, objectiveDefinition.requiredAmount);
                }
            }

            CheckQuestCompletion(runtime);
        }

        private void CheckQuestCompletion(QuestRuntime runtime)
        {
            for (int i = 0; i < runtime.Definition.objectives.Count; i++)
            {
                var current =
                    runtime.Definition.questType == QuestType.Main ? 
                    Session.GetObjectiveProgress(runtime.QuestId, i) 
                    : runtime.State.objectives[i].currentAmount;

                int required = runtime.Definition.objectives[i].requiredAmount;

                if (current < required)
                    return;
            }

            runtime.State.isCompleted = true;

            _completedQuests.Add(runtime.QuestId);

            if (runtime.Definition.questType == QuestType.Main) 
            { Session?.MarkMainQuestCompleted(runtime.QuestId); }
            
            RPC_ShowQuestNotification(
                (int)QuestNotificationType.Completed,
                runtime.Definition.questName);

            EventController.Instance.TriggerEvent(new QuestUIRefreshEvent());
        }

        private void AcceptMainQuestShared(QuestDefinitionSO definition)
        {
            if (Session == null)
                return;

            Session.MarkMainQuestAccepted(definition.questId);

            AddQuestLocally(definition);
            
            RPC_ShowQuestNotification(
                (int)QuestNotificationType.Accepted,
                definition.questName);
        }

        private void AddQuestLocally(QuestDefinitionSO definition)
        {
            if (_activeQuests.ContainsKey(definition.questId)) { return; }

            QuestRuntime runtime = new QuestRuntime(definition);

            if (Session != null)
            {
                for (int i = 0; i < runtime.State.objectives.Count; i++)
                {
                    runtime.State.objectives[i].currentAmount =
                        Session.GetObjectiveProgress(definition.questId, i);
                }

                if (Session.IsMainQuestCompleted(definition.questId))
                { runtime.State.isCompleted = true; }
            }

            _activeQuests.Add(definition.questId, runtime);
            
            EventController.Instance.TriggerEvent(new QuestUIRefreshEvent());
        }
        
        private void GiveRewards(PlayerRef playerRef, QuestDefinitionSO definition)
        {
            bool found = Runner.TryGetPlayerObject(playerRef, out NetworkObject playerObject);
            if (!found)
                return;
            
            var inventory = playerObject.GetComponent<NetworkInventorySystem>();

            var exp = playerObject.GetComponent<NetworkExperienceSystem>();

            foreach (var reward in definition.rewards)
            {
                if (!string.IsNullOrWhiteSpace(reward.itemId))
                {
                    int itemId = ItemDatabase.Instance.GetNetworkId(reward.itemId);

                    if (itemId > 0)
                    {
                        inventory.Server_AddItemGlobal(itemId, reward.quantity);
                    }
                }

                if (reward.experience > 0 && exp != null)
                {
                    exp.Server_AddXP(reward.experience);
                }
            }
        }
    }
}