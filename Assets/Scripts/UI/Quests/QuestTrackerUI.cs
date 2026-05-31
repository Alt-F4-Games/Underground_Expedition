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

            if (NetworkQuestManager.Local == null)
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
            Clear();

            if (!NetworkQuestManager.Local)
            {
                Debug.LogWarning(
                    "[QuestTrackerUI] Local QuestManager null");

                return;
            }

            foreach (var pair
                     in NetworkQuestManager
                         .Local
                         .ActiveQuests)
            {
                QuestRuntime runtime =
                    pair.Value;

                bool claimed =
                    NetworkQuestManager.Local
                        .IsQuestRewardClaimed(
                            runtime.QuestId);
                
                if (claimed)
                    continue;

                CreateEntry(runtime);
            }
        }

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

        private void Clear()
        {
            foreach (Transform child
                     in content)
            {
                Destroy(child.gameObject);
            }

            _entries.Clear();
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