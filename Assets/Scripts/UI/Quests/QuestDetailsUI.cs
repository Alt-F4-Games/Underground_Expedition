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

        [SerializeField]
        private TMP_Text questTypeText;

        public void Clear()
        {
            titleText.text = "";
            descriptionText.text = "";
            objectivesText.text = "";
            rewardsText.text = "";

            if (questTypeText != null)
            {
                questTypeText.text = "";
            }
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

            if (questTypeText != null)
            {
                questTypeText.text =
                    definition.questType ==
                    Network.Quests.Enums.QuestType.Main
                        ? "Main Quest (Shared)"
                        : "Secondary Quest (Personal)";
            }

            BuildObjectives(
                definition,
                runtime);

            BuildRewards(
                definition);
        }

        public void ShowLockedQuest(
            QuestDefinitionSO definition,
            string requiredQuestName)
        {
            titleText.text = "???";

            descriptionText.text =
                "This quest is locked.";

            objectivesText.text =
                $"Complete:\n{requiredQuestName}\n\nto unlock this quest.";

            rewardsText.text = "";

            if (questTypeText != null)
            {
                questTypeText.text = "Locked";
            }
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