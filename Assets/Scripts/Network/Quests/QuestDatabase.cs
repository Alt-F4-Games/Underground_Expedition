using System.Collections.Generic;
using Network.Quests.Definitions;
using UnityEngine;

namespace Network.Quests
{
    [CreateAssetMenu(menuName = "Quests/Quest Database")]
    public class QuestDatabase : ScriptableObject
    {
        public static QuestDatabase Instance { get; private set; }

        [SerializeField]
        private List<QuestDefinitionSO> quests = new();

        private Dictionary<string, QuestDefinitionSO> _lookup;

        public IReadOnlyList<QuestDefinitionSO> Quests => quests;

        public void Initialize()
        {
            Instance = this;

            _lookup = new Dictionary<string, QuestDefinitionSO>();

            foreach (var quest in quests)
            {
                if (quest == null)
                    continue;

                if (string.IsNullOrWhiteSpace(quest.questId))
                    continue;

                if (_lookup.ContainsKey(quest.questId))
                    continue;

                _lookup.Add(quest.questId, quest);
            }

            Debug.Log($"[QuestDatabase] Initialized: {_lookup.Count} quests");
        }

        public QuestDefinitionSO GetQuestById(string questId)
        {
            if (_lookup == null)
                Initialize();

            _lookup.TryGetValue(questId, out var quest);
            return quest;
        }

        public bool Exists(string questId)
        {
            return GetQuestById(questId) != null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Load()
        {
            var db = Resources.Load<QuestDatabase>("QuestDatabase");

            if (db != null)
                db.Initialize();
        }
    }
}