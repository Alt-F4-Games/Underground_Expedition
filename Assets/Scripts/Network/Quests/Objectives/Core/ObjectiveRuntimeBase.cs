using Events;
using Network.Quests.Definitions;
using Network.Quests.Interfaces;
using Network.Quests.Runtime;
using Network.Quests.Services;
using Tools.EventSystem;

namespace Network.Quests.Objectives.Core
{
    public abstract class ObjectiveRuntimeBase
        : IQuestObjective
    {
        protected QuestRuntime ParentQuest;

        public QuestObjectiveDefinition Definition;

        protected int StepIndex;

        protected int ObjectiveIndex;
        
        public QuestObjectiveDefinition DefinitionData =>
            Definition;

        protected ObjectiveRuntimeBase(
            QuestRuntime quest,
            QuestObjectiveDefinition definition,
            int stepIndex,
            int objectiveIndex)
        {
            ParentQuest = quest;
            Definition = definition;
            StepIndex = stepIndex;
            ObjectiveIndex = objectiveIndex;

            EnsureState();
        }

        // =====================================================
        // STATE
        // =====================================================

        protected QuestObjectiveState State =>
            ParentQuest.State
                .Steps[StepIndex]
                .Objectives[ObjectiveIndex];

        private void EnsureState()
        {
            while (ParentQuest.State.Steps.Count <= StepIndex)
            {
                ParentQuest.State.Steps.Add(
                    new QuestStepState());
            }

            QuestStepState step =
                ParentQuest.State.Steps[StepIndex];

            while (step.Objectives.Count <= ObjectiveIndex)
            {
                step.Objectives.Add(
                    new QuestObjectiveState());
            }
        }

        // =====================================================
        // LIFECYCLE
        // =====================================================

        public abstract void Initialize();

        public abstract void Dispose();

        // =====================================================
        // PROGRESS
        // =====================================================

        protected void AddProgress(int amount)
        {
            State.CurrentAmount += amount;

            EventController.Instance.TriggerEvent(
                new QuestObjectiveProgressEvent
                {
                    quest = ParentQuest,
                    StepIndex = StepIndex,
                    ObjectiveIndex = ObjectiveIndex,
                    CurrentAmount = State.CurrentAmount,
                    RequiredAmount = Definition.requiredAmount
                });

            EvaluateCompletion();
        }

        protected void SetProgress(int amount)
        {
            State.CurrentAmount = amount;

            EventController.Instance.TriggerEvent(
                new QuestObjectiveProgressEvent
                {
                    quest = ParentQuest,
                    StepIndex = StepIndex,
                    ObjectiveIndex = ObjectiveIndex,
                    CurrentAmount = State.CurrentAmount,
                    RequiredAmount = Definition.requiredAmount
                });

            EvaluateCompletion();
        }

        public virtual bool IsCompleted()
        {
            return State.CurrentAmount >=
                   Definition.requiredAmount;
        }

        public virtual float GetProgress()
        {
            if (Definition.requiredAmount <= 0)
                return 0f;

            return (float)State.CurrentAmount /
                   Definition.requiredAmount;
        }

        public int GetCurrentAmount()
        {
            return State.CurrentAmount;
        }

        // =====================================================
        // COMPLETION
        // =====================================================

        protected void EvaluateCompletion()
        {
            if (!IsCompleted())
                return;

            QuestStepService.EvaluateStep(
                ParentQuest);
        }
    }
}