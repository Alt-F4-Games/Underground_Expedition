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

        private bool _initialized;

        private void Start()
        {
            Subscribe();
        }

        private void Update()
        {
            if (_initialized)
                return;

            if (!NetworkQuestManager.Local)
                return;

            Build();

            _initialized = true;
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Build()
        {
            if (!NetworkQuestManager.Local)
                return;

            foreach (var pair
                     in NetworkQuestManager
                         .Local
                         .ActiveQuests)
            {
                CreateOrRefreshEntry(
                    pair.Value);
            }
        }

        private void CreateOrRefreshEntry(
            QuestRuntime runtime)
        {
            bool claimed =
                NetworkQuestManager.Local
                    .IsQuestRewardClaimed(
                        runtime.QuestId);

            if (claimed)
            {
                RemoveEntry(
                    runtime.QuestId);

                return;
            }

            if (_entries.TryGetValue(
                    runtime.QuestId,
                    out QuestTrackerEntryUI existing))
            {
                existing.Refresh();
                return;
            }

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
            if (!_entries.TryGetValue(
                    questId,
                    out QuestTrackerEntryUI entry))
            {
                return;
            }

            Destroy(entry.gameObject);

            _entries.Remove(
                questId);
        }

        private void Subscribe()
        {
            EventController.Instance
                .AddListener<QuestUIRefreshEvent>(
                    OnRefresh);
        }

        private void Unsubscribe()
        {
            EventController.Instance
                .RemoveListener<QuestUIRefreshEvent>(
                    OnRefresh);
        }

        private void OnRefresh(
            QuestUIRefreshEvent evt)
        {
            Build();
        }
    }
}