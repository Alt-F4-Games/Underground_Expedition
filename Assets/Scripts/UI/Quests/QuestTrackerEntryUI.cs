// =====================================================
// QuestTrackerEntryUI.cs
// =====================================================

using System.Collections.Generic;
using Network.Quests.Definitions;
using Network.Quests.Runtime;
using TMPro;
using UnityEngine;

namespace UI.Quests
{
    public class QuestTrackerEntryUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text questNameText;

        [SerializeField]
        private Transform objectiveContainer;

        [SerializeField]
        private ObjectiveEntryUI objectivePrefab;

        private readonly List<ObjectiveEntryUI>
            _entries = new();

        private QuestRuntime _runtime;

        public void Bind(
            QuestRuntime runtime)
        {
            _runtime = runtime;

            Refresh();
        }

        public void Refresh()
        {
            if (_runtime == null)
                return;

            QuestDefinitionSO definition =
                _runtime.Definition;

            questNameText.text =
                definition.questName;

            ClearObjectives();

            for (int i = 0;
                 i < definition.objectives.Count;
                 i++)
            {
                var objective =
                    definition.objectives[i];

                int current =
                    _runtime.State
                        .objectives[i]
                        .currentAmount;

                bool completed =
                    current >=
                    objective.requiredAmount;

                ObjectiveEntryUI entry =
                    Instantiate(
                        objectivePrefab,
                        objectiveContainer);

                entry.SetData(
                    objective.displayName,
                    current,
                    objective.requiredAmount,
                    completed);

                _entries.Add(entry);
            }
        }

        private void ClearObjectives()
        {
            foreach (var entry
                     in _entries)
            {
                Destroy(entry.gameObject);
            }

            _entries.Clear();
        }
    }
}