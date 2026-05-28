using Network.Quests.Definitions;

namespace Network.Quests.Runtime
{
    public class QuestRuntime
    {
        public QuestDefinitionSO Definition { get; private set; }
        public QuestState State { get; private set; }

        public string QuestId => Definition.questId;
        public string QuestName => Definition.questName;

        public QuestRuntime(QuestDefinitionSO definition)
        {
            Definition = definition;

            State = new QuestState
            {
                questId = definition.questId,
                currentStepIndex = 0,
                isCompleted = false
            };

            BuildState();
        }

        public QuestRuntime(
            QuestDefinitionSO definition,
            QuestState state)
        {
            Definition = definition;
            State = state;
        }

        private void BuildState()
        {
            for (int i = 0; i < Definition.steps.Count; i++)
            {
                QuestStepState stepState = new();

                for (int j = 0;
                     j < Definition.steps[i].objectives.Count;
                     j++)
                {
                    stepState.objectives.Add(
                        new QuestObjectiveState
                        {
                            currentAmount = 0
                        });
                }

                State.steps.Add(stepState);
            }
        }

        public bool IsQuestFinished()
        {
            return State.isCompleted;
        }

        public bool HasPlayerClaimed(string playerId)
        {
            return State.claimedPlayerIds.Contains(playerId);
        }

        public void MarkRewardClaimed(string playerId)
        {
            if (!State.claimedPlayerIds.Contains(playerId))
            {
                State.claimedPlayerIds.Add(playerId);
            }
        }
    }
}