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

        private Dictionary<string, QuestDefinitionSO> _lookup = new();
        
        public IReadOnlyList<QuestDefinitionSO> Quests => quests;

        public void Initialize()
        {
            Instance = this;

            _lookup.Clear();

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

            Debug.Log(
                $"[QuestDatabase] Initialized with {_lookup.Count} quests.");
        }

        public QuestDefinitionSO GetQuestById(string questId)
        {
            return _lookup.GetValueOrDefault(questId);
        }

        public List<QuestDefinitionSO> GetAllQuests()
        {
            return quests;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadDatabase()
        {
            var db =
                Resources.Load<QuestDatabase>("QuestDatabase");

            if (db != null)
            {
                db.Initialize();
            }
        }
    }
}