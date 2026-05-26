using Events;
using System.Collections.Generic;
using Network.Quests.Definitions;
using Network.Quests.Enums;
using Tools.EventSystem;

namespace Network.Quests.Runtime
{
    public class QuestRuntime
    {
        public QuestDefinitionSO Definition
        { get; private set; }

        public QuestRuntimeState State
        { get; private set; }

        public QuestStatus Status
        { get; private set; }

        public int CurrentStepIndex =>
            State.CurrentStepIndex;

        public List<QuestStepRuntime> Steps
        { get; private set; } = new();

        public QuestStepRuntime CurrentStep =>
            CurrentStepIndex >= Steps.Count
                ? null
                : Steps[CurrentStepIndex];
        
        public string QuestId =>
            Definition.questId;

        public string QuestName =>
            Definition.questName;

        public IReadOnlyList<QuestStepRuntime>
            StepRuntimes =>
            Steps;

        public QuestRuntime(
            QuestDefinitionSO definition,
            QuestRuntimeState state = null)
        {
            Definition = definition;

            State = state ??
                    new QuestRuntimeState
                    {
                        QuestId = definition.questId
                    };

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

            State.CurrentStepIndex++;

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

            State.IsCompleted = true;

            EventController.Instance.TriggerEvent(
                new QuestCompletedEvent
                {
                    quest = this
                });
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

        public bool IsCurrentStepCompleted()
        {
            return CurrentStep != null &&
                   CurrentStep.IsCompleted();
        }
    }
}