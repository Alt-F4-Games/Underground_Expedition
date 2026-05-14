using Network.Quests.Definitions;
using Network.Quests.Interfaces;
using Network.Quests.Runtime;

namespace Quests.Objectives
{
    public abstract class ObjectiveRuntimeBase : IQuestObjective
    {
        protected QuestRuntime Quest;
        protected QuestObjectiveDefinition Definition;
        protected int StepIndex;

        protected int CurrentAmount;

        protected ObjectiveRuntimeBase(
            QuestRuntime quest,
            QuestObjectiveDefinition definition,
            int stepIndex)
        {
            Quest = quest;
            Definition = definition;
            StepIndex = stepIndex;
        }

        public abstract void Initialize();

        public abstract void Dispose();

        public virtual bool IsCompleted()
        {
            return CurrentAmount >= Definition.requiredAmount;
        }

        public virtual float GetProgress()
        {
            if (Definition.requiredAmount <= 0)
                return 0f;

            return (float) CurrentAmount / Definition.requiredAmount;
        }

        protected void CompleteObjective()
        {
            Quest.CompleteCurrentStep();
        }
    }
}