using System.Collections.Generic;
using Network.Quests.Runtime;
using UnityEngine;

namespace Network.Quests
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance
        { get; private set; }

        private readonly Dictionary<string,
            QuestRuntime> _activeQuests = new();

        private readonly HashSet<string>
            _completedQuests = new();

        private void Awake()
        {
            Instance = this;
        }

        public void RegisterQuest(
            QuestRuntime runtime)
        {
            if (runtime == null)
                return;

            string questId =
                runtime.Definition.questId;

            if (_activeQuests.ContainsKey(
                    questId))
                return;

            _activeQuests.Add(
                questId,
                runtime);
        }

        public bool HasQuest(string questId)
        {
            return _activeQuests.ContainsKey(
                questId);
        }

        public QuestRuntime GetQuest(
            string questId)
        {
            return _activeQuests
                .GetValueOrDefault(questId);
        }

        public List<QuestRuntime> GetAllActiveQuests()
        {
            return new List<QuestRuntime>(
                _activeQuests.Values);
        }

        public void CompleteQuest(
            string questId)
        {
            if (!_activeQuests.Remove(questId,
                    out var runtime))
                return;

            if (runtime.IsQuestFinished())
            {
                _completedQuests.Add(
                    questId);
            }

            runtime.Dispose();
        }

        public bool IsQuestCompleted(
            string questId)
        {
            return _completedQuests.Contains(
                questId);
        }
        
        public bool TryGetQuest(
            string questId,
            out QuestRuntime runtime)
        {
            return _activeQuests.TryGetValue(
                questId,
                out runtime);
        } 
        
    // =====================================================
    // READONLY ACCESS
    // =====================================================

        public IReadOnlyDictionary<string, QuestRuntime>
            ActiveQuestMap =>
            _activeQuests;
        
        public IReadOnlyCollection<QuestRuntime>
            ActiveQuests =>
            _activeQuests.Values;

        public IReadOnlyCollection<string>
            CompletedQuestIds =>
            _completedQuests;
        
        public void MarkQuestCompleted(
            string questId)
        {
            _completedQuests.Add(
                questId);
        }
        
        public void ClearAll()
        {
            foreach (var runtime
                     in _activeQuests.Values)
            {
                runtime.Dispose();
            }

            _activeQuests.Clear();

            _completedQuests.Clear();
        }
    }
}