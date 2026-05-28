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

            foreach (var step in definition.steps)
            {
                foreach (var objective
                         in step.objectives)
                {
                    builder.AppendLine(
                        objective.description);
                }
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