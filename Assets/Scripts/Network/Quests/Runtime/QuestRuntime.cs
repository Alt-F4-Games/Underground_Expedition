using System.Collections.Generic;
using Network.Quests.Definitions;

namespace Network.Quests.Runtime
{
    public class QuestRuntime
    {
        // =====================================================
        // DEFINITION
        // =====================================================

        public QuestDefinitionSO Definition
            { get; private set; }

        // =====================================================
        // STATE
        // =====================================================

        public QuestStatus Status
            { get; private set; } =
                QuestStatus.Locked;

        // =====================================================
        // STEPS
        // =====================================================

        public List<QuestStepRuntime> Steps
            { get; private set; } = new();

        public int CurrentStepIndex
            { get; private set; }

        public QuestStepRuntime CurrentStep =>
            CurrentStepIndex >= Steps.Count
                ? null
                : Steps[CurrentStepIndex];

        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public QuestRuntime(
            QuestDefinitionSO definition)
        {
            Definition = definition;

            BuildSteps();
        }

        // =====================================================
        // INITIALIZATION
        // =====================================================

        private void BuildSteps()
        {
            Steps.Clear();

            for (int i = 0;
                 i < Definition.steps.Count;
                 i++)
            {
                var runtime =
                    new QuestStepRuntime(
                        this,
                        Definition.steps[i],
                        i);

                Steps.Add(runtime);
            }
        }

        // =====================================================
        // QUEST FLOW
        // =====================================================

        public void MakeAvailable()
        {
            Status = QuestStatus.Available;
        }

        public void AcceptQuest()
        {
            Status = QuestStatus.Accepted;

            StartQuest();
        }

        public void StartQuest()
        {
            Status = QuestStatus.Active;

            CurrentStep?.Initialize();
        }

        public void CancelQuest()
        {
            if (!Definition.canCancel)
                return;

            CurrentStep?.Dispose();

            Status = QuestStatus.Cancelled;
        }

        // =====================================================
        // STEP FLOW
        // =====================================================

        public void AdvanceStep()
        {
            CurrentStep?.Dispose();

            CurrentStepIndex++;

            if (CurrentStepIndex >= Steps.Count)
            {
                CompleteObjectives();
                return;
            }

            CurrentStep.Initialize();
        }

        // =====================================================
        // COMPLETION
        // =====================================================

        private void CompleteObjectives()
        {
            if (Definition.requiresNpcToComplete)
            {
                Status = QuestStatus.RewardPending;
            }
            else
            {
                ClaimRewards();
            }
        }

        public void ClaimRewards()
        {
            if (Status != QuestStatus.RewardPending &&
                Status != QuestStatus.Active)
                return;

            QuestRewardService.GiveRewards(this);

            Status = QuestStatus.RewardClaimed;

            CompleteQuest();
        }

        private void CompleteQuest()
        {
            Status = QuestStatus.Completed;
            QuestManager.Instance.CompleteQuest(this);
        }

        // =====================================================
        // CLEANUP
        // =====================================================

        public void Dispose()
        {
            CurrentStep?.Dispose();
        }
    }
}