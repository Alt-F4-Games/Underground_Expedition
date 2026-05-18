using System.Collections.Generic;
using Network.Quests.Definitions;
using Network.Quests.Enums;

namespace Network.Quests.Runtime
{
    public class QuestRuntime
    {
        public QuestDefinitionSO Definition
        { get; private set; }

        public QuestStatus Status
        { get; private set; }

        public int CurrentStepIndex
        { get; private set; }

        public List<QuestStepRuntime> Steps
        { get; private set; } = new();

        public QuestStepRuntime CurrentStep =>
            CurrentStepIndex >= Steps.Count
                ? null
                : Steps[CurrentStepIndex];

        public QuestRuntime(
            QuestDefinitionSO definition)
        {
            Definition = definition;

            BuildSteps();
        }

        // =====================================================
        // BUILD
        // =====================================================

        private void BuildSteps()
        {
            Steps.Clear();

            for (int i = 0;
                 i < Definition.steps.Count;
                 i++)
            {
                Steps.Add(
                    new QuestStepRuntime(
                        this,
                        Definition.steps[i],
                        i));
            }
        }

        // =====================================================
        // QUEST FLOW
        // =====================================================

        public void StartQuest()
        {
            Status = QuestStatus.InProgress;

            CurrentStep?.Initialize();
        }

        public void Dispose()
        {
            CurrentStep?.Dispose();
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
                CompleteQuest();
                return;
            }

            CurrentStep?.Initialize();
        }

        // =====================================================
        // COMPLETION
        // =====================================================

        private void CompleteQuest()
        {
            Status = QuestStatus.Completed;
        }

        // =====================================================
        // STATE
        // =====================================================

        public bool IsQuestFinished()
        {
            return Status == QuestStatus.Completed;
        }

        public void SetStatus(
            QuestStatus status)
        {
            Status = status;
        }
    }
}