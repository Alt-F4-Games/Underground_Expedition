using System.Collections.Generic;
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
            _objectives = new();

        private QuestRuntime _runtime;

        // =====================================================
        // SETUP
        // =====================================================

        public void Bind(
            QuestRuntime runtime)
        {
            _runtime = runtime;

            Refresh();
        }

        // =====================================================
        // REFRESH
        // =====================================================

        public void Refresh()
        {
            if (_runtime == null)
                return;

            questNameText.text =
                _runtime.QuestName;

            ClearObjectives();

            QuestStepRuntime step =
                _runtime.CurrentStep;

            if (step == null)
                return;

            foreach (var objective
                     in step.ObjectiveRuntimes)
            {
                ObjectiveEntryUI entry =
                    Instantiate(
                        objectivePrefab,
                        objectiveContainer);

                entry.SetData(
                    objective.DefinitionData.description,
                    objective.GetCurrentAmount(),
                    objective.DefinitionData.requiredAmount,
                    objective.IsCompleted());

                _objectives.Add(entry);
            }
        }

        // =====================================================
        // CLEANUP
        // =====================================================

        private void ClearObjectives()
        {
            foreach (var entry in _objectives)
            {
                Destroy(entry.gameObject);
            }

            _objectives.Clear();
        }
    }
}