using System.Collections.Generic;
using Network.Quests.Runtime;
using UnityEngine;

namespace Network.Quests
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance
            { get; private set; }

        // =====================================================
        // QUEST STORAGE
        // =====================================================

        private readonly Dictionary<string, QuestRuntime>
            _activeQuests = new();

        private readonly Dictionary<string, QuestRuntime>
            _completedQuests = new();

        private void Awake()
        {
            Instance = this;
        }

        // =====================================================
        // QUEST CREATION
        // =====================================================

        public bool RegisterQuest(string questId)
        {
            if (_activeQuests.ContainsKey(questId))
                return false;

            var definition =
                QuestDatabase.Instance.GetQuestById(
                    questId);

            if (definition == null)
            {
                Debug.LogWarning(
                    $"Quest not found: {questId}");

                return false;
            }

            var runtime =
                new QuestRuntime(definition);

            runtime.MakeAvailable();

            _activeQuests.Add(
                questId,
                runtime);

            return true;
        }

        // =====================================================
        // ACCEPT
        // =====================================================

        public bool AcceptQuest(string questId)
        {
            if (!_activeQuests.TryGetValue(
                    questId,
                    out var runtime))
                return false;

            if (runtime.Status !=
                QuestStatus.Available)
                return false;

            runtime.AcceptQuest();

            return true;
        }

        // =====================================================
        // CANCEL
        // =====================================================

        public bool CancelQuest(string questId)
        {
            if (!_activeQuests.TryGetValue(
                    questId,
                    out var runtime))
                return false;

            runtime.CancelQuest();

            return true;
        }

        // =====================================================
        // COMPLETE
        // =====================================================

        public void CompleteQuest(
            QuestRuntime runtime)
        {
            string questId =
                runtime.Definition.questId;

            if (!_activeQuests.ContainsKey(
                    questId))
                return;

            runtime.ClaimRewards();

            _activeQuests.Remove(
                questId);

            _completedQuests.TryAdd(
                questId,
                runtime);
        }

        // =====================================================
        // LOOKUP
        // =====================================================

        public bool IsQuestActive(
            string questId)
        {
            return _activeQuests.ContainsKey(
                questId);
        }

        public bool IsQuestCompleted(
            string questId)
        {
            return _completedQuests.ContainsKey(
                questId);
        }

        public QuestRuntime GetQuest(
            string questId)
        {
            return _activeQuests.GetValueOrDefault(
                questId);
        }

        public IReadOnlyDictionary<string, QuestRuntime>
            GetActiveQuests()
        {
            return _activeQuests;
        }

        public IReadOnlyDictionary<string, QuestRuntime>
            GetCompletedQuests()
        {
            return _completedQuests;
        }
    }
}