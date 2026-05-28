using System.Collections.Generic;
using Network.Quests.Definitions;
using Network.Quests.Runtime;
using TMPro;
using UnityEngine;

namespace UI.Quests
{
    public class QuestTrackerEntryUI : MonoBehaviour
    {
        [Header("References")]
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

            int currentStep =
                _runtime.State.currentStepIndex;

            if (currentStep >= definition.steps.Count)
                return;

            var step =
                definition.steps[currentStep];

            for (int i = 0;
                 i < step.objectives.Count;
                 i++)
            {
                var objective =
                    step.objectives[i];

                int current =
                    _runtime.State
                        .steps[currentStep]
                        .objectives[i]
                        .currentAmount;

                bool completed =
                    current >= objective.requiredAmount;

                ObjectiveEntryUI entry =
                    Instantiate(
                        objectivePrefab,
                        objectiveContainer);

                entry.SetData(
                    objective.description,
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