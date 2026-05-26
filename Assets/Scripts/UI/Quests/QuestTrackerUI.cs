using System.Collections.Generic;
using Events;
using Network.Quests;
using Network.Quests.Runtime;
using Tools.EventSystem;
using UnityEngine;

namespace UI.Quests
{
    public class QuestTrackerUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform content;

        [SerializeField]
        private QuestTrackerEntryUI entryPrefab;

        private readonly Dictionary<string,
            QuestTrackerEntryUI>
            _entries = new();

        // =====================================================
        // UNITY
        // =====================================================

        private void Start()
        {
            BuildInitialUI();

            SubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        // =====================================================
        // BUILD
        // =====================================================

        private void BuildInitialUI()
        {
            foreach (var runtime
                     in QuestManager.Instance.ActiveQuests)
            {
                CreateEntry(runtime);
            }
        }

        // =====================================================
        // EVENTS
        // =====================================================

        private void SubscribeEvents()
        {
            EventController.Instance
                .AddListener<QuestAcceptedEvent>(
                    OnQuestAccepted);

            EventController.Instance
                .AddListener<QuestCompletedEvent>(
                    OnQuestCompleted);

            EventController.Instance
                .AddListener<QuestObjectiveProgressEvent>(
                    OnObjectiveProgress);
        }

        private void UnsubscribeEvents()
        {
            EventController.Instance
                .RemoveListener<QuestAcceptedEvent>(
                    OnQuestAccepted);

            EventController.Instance
                .RemoveListener<QuestCompletedEvent>(
                    OnQuestCompleted);

            EventController.Instance
                .RemoveListener<QuestObjectiveProgressEvent>(
                    OnObjectiveProgress);
        }

        // =====================================================
        // EVENT CALLBACKS
        // =====================================================

        private void OnQuestAccepted(
            QuestAcceptedEvent evt)
        {
            CreateEntry(evt.quest);
        }

        private void OnQuestCompleted(
            QuestCompletedEvent evt)
        {
            RemoveEntry(
                evt.quest.QuestId);
        }

        private void OnObjectiveProgress(
            QuestObjectiveProgressEvent evt)
        {
            if (!_entries.TryGetValue(
                    evt.quest.QuestId,
                    out var entry))
                return;

            entry.Refresh();
        }

        // =====================================================
        // ENTRY MANAGEMENT
        // =====================================================

        private void CreateEntry(
            QuestRuntime runtime)
        {
            if (_entries.ContainsKey(
                    runtime.QuestId))
                return;

            QuestTrackerEntryUI entry =
                Instantiate(
                    entryPrefab,
                    content);

            entry.Bind(runtime);

            _entries.Add(
                runtime.QuestId,
                entry);
        }

        private void RemoveEntry(
            string questId)
        {
            if (!_entries.Remove(
                    questId,
                    out var entry))
                return;

            Destroy(entry.gameObject);
        }
    }
}