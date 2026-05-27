using System.Text;
using Network.Quests.Definitions;
using Network.Quests.Runtime;
using TMPro;
using UnityEngine;

namespace UI.Quests
{
    public class QuestDetailsUI : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text descriptionText;

        [SerializeField]
        private TMP_Text objectivesText;

        [SerializeField]
        private TMP_Text rewardsText;

        // =====================================================
        // DISPLAY
        // =====================================================

        public void ShowQuest(
            QuestDefinitionSO definition,
            QuestRuntime runtime)
        {
            if (definition == null)
                return;

            titleText.text =
                definition.questName;

            descriptionText.text =
                definition.description;

            BuildObjectives(
                definition,
                runtime);

            BuildRewards(
                definition);
        }

        // =====================================================
        // OBJECTIVES
        // =====================================================

        private void BuildObjectives(
            QuestDefinitionSO definition,
            QuestRuntime runtime)
        {
            StringBuilder builder =
                new();

            for (int i = 0;
                 i < definition.steps.Count;
                 i++)
            {
                var step =
                    definition.steps[i];

                foreach (var objective
                         in step.objectives)
                {
                    int current = 0;

                    if (runtime != null &&
                        i < runtime.StepRuntimes.Count)
                    {
                        var runtimeStep =
                            runtime.StepRuntimes[i];

                        foreach (var objectiveRuntime
                                 in runtimeStep.ObjectiveRuntimes)
                        {
                            if (objectiveRuntime.DefinitionData
                                == objective)
                            {
                                current =
                                    objectiveRuntime
                                        .GetCurrentAmount();
                            }
                        }
                    }

                    builder.AppendLine(
                        $"{objective.description} ({current}/{objective.requiredAmount})");
                }
            }

            objectivesText.text =
                builder.ToString();
        }

        // =====================================================
        // REWARDS
        // =====================================================

        private void BuildRewards(
            QuestDefinitionSO definition)
        {
            StringBuilder builder =
                new();

            foreach (var reward
                     in definition.rewards)
            {
                builder.AppendLine(
                    $"{reward.quantity}x {reward.itemId}");
            }

            rewardsText.text =
                builder.ToString();
        }
    }
}