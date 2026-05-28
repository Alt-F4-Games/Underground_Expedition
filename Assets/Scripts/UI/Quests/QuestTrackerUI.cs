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

        private void Start()
        {
            Build();

            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Build()
        {
            Clear();

            foreach (var pair
                     in NetworkQuestManager
                         .Local
                         .ActiveQuests)
            {
                CreateEntry(pair.Value);
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