using Network.Quests.Definitions;
using Network.Quests.Enums;

namespace Network.Quests.Runtime
{
    public class QuestRuntime
    {
        public QuestDefinitionSO Definition { get; private set; }

        public QuestStatus Status { get; private set; }

        public int CurrentStepIndex { get; private set; }

        public QuestStepRuntime CurrentStep { get; private set; }

        public bool IsCompleted =>
            Status == QuestStatus.Completed ||
            Status == QuestStatus.RewardPending ||
            Status == QuestStatus.RewardClaimed;

        public QuestRuntime(QuestDefinitionSO definition)
        {
            Definition = definition;

            Status = QuestStatus.Available;

            CurrentStepIndex = 0;

            BuildCurrentStep();
        }

        public void AcceptQuest()
        {
            if (Status != QuestStatus.Available)
                return;

            Status = QuestStatus.Accepted;

            CurrentStep.Initialize();
        }

        public void CompleteCurrentStep()
        {
            CurrentStep.Dispose();

            CurrentStepIndex++;

            if (CurrentStepIndex >= Definition.steps.Count)
            {
                Status = QuestStatus.RewardPending;
                return;
            }

            BuildCurrentStep();

            CurrentStep.Initialize();
        }

        public void ClaimReward()
        {
            if (Status != QuestStatus.RewardPending)
                return;

            Status = QuestStatus.RewardClaimed;
        }

        public void CancelQuest()
        {
            if (!Definition.canCancel)
                return;

            Status = QuestStatus.Cancelled;

            CurrentStep.Dispose();
        }

        private void BuildCurrentStep()
        {
            CurrentStep = new QuestStepRuntime(
                this,
                Definition.steps[CurrentStepIndex],
                CurrentStepIndex);
        }
    }
}