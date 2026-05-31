// =====================================================
// QuestDetailsUI.cs
// =====================================================

using System.Text;
using Network.Quests.Definitions;
using Network.Quests.Runtime;
using TMPro;
using UnityEngine;

namespace UI.Quests
{
    public class QuestDetailsUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text titleText;

        [SerializeField]
        private TMP_Text descriptionText;

        [SerializeField]
        private TMP_Text objectivesText;

        [SerializeField]
        private TMP_Text rewardsText;

        public void Clear()
        {
            titleText.text = "";
            descriptionText.text = "";
            objectivesText.text = "";
            rewardsText.text = "";
        }

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

        private void BuildObjectives(
            QuestDefinitionSO definition,
            QuestRuntime runtime)
        {
            StringBuilder builder =
                new();

            for (int i = 0;
                 i < definition.objectives.Count;
                 i++)
            {
                var objective =
                    definition.objectives[i];

                int current = 0;

                if (runtime != null)
                {
                    current =
                        runtime.State
                            .objectives[i]
                            .currentAmount;
                }

                builder.AppendLine(
                    $"{objective.displayName} ({current}/{objective.requiredAmount})");
            }

            objectivesText.text =
                builder.ToString();
        }

        private void BuildRewards(
            QuestDefinitionSO definition)
        {
            StringBuilder builder =
                new();

            foreach (var reward
                     in definition.rewards)
            {
                if (reward.quantity > 0)
                {
                    builder.AppendLine(
                        $"{reward.quantity}x {reward.itemId}");
                }

                if (reward.experience > 0)
                {
                    builder.AppendLine(
                        $"{reward.experience} XP");
                }
            }

            rewardsText.text =
                builder.ToString();
        }
    }
}