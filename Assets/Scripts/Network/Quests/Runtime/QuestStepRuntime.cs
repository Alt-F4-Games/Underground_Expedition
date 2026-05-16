using Network.Quests.Definitions;
using Network.Quests.Objectives.Core;
using Quests.Objectives;

namespace Network.Quests.Runtime
{
    public class QuestStepRuntime
    {
        public QuestRuntime ParentQuest { get; private set; }

        public QuestStepDefinition Definition { get; private set; }

        public int StepIndex { get; private set; }

        public ObjectiveRuntimeBase ObjectiveRuntime { get; private set; }

        public QuestStepRuntime(
            QuestRuntime parentQuest,
            QuestStepDefinition definition,
            int stepIndex)
        {
            ParentQuest = parentQuest;
            Definition = definition;
            StepIndex = stepIndex;

            BuildObjective();
        }

        public void Initialize()
        {
            ObjectiveRuntime.Initialize();
        }

        public void Dispose()
        {
            ObjectiveRuntime.Dispose();
        }

        private void BuildObjective()
        {
            ObjectiveRuntime =
                ObjectiveFactory.CreateObjective(
                    ParentQuest,
                    Definition.objective,
                    StepIndex);
        }
    }
}