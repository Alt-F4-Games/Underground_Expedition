using System.Collections.Generic;
using Network.Quests.Definitions;
using Network.Quests.Objectives.Core;

namespace Network.Quests.Runtime
{
    public class QuestStepRuntime
    {
        public QuestRuntime ParentQuest
        { get; private set; }

        public QuestStepDefinition Definition
        { get; private set; }

        public int StepIndex
        { get; private set; }

        public List<ObjectiveRuntimeBase>
            ObjectiveRuntimes
            { get; private set; } = new();

        public QuestStepRuntime(
            QuestRuntime parentQuest,
            QuestStepDefinition definition,
            int stepIndex)
        {
            ParentQuest = parentQuest;
            Definition = definition;
            StepIndex = stepIndex;

            BuildObjectives();
        }

        // =====================================================
        // LIFECYCLE
        // =====================================================

        public void Initialize()
        {
            foreach (var runtime in ObjectiveRuntimes)
            {
                runtime.Initialize();
            }
        }

        public void Dispose()
        {
            foreach (var runtime in ObjectiveRuntimes)
            {
                runtime.Dispose();
            }
        }

        // =====================================================
        // BUILD
        // =====================================================

        private void BuildObjectives()
        {
            ObjectiveRuntimes.Clear();

            for (int i = 0;
                 i < Definition.objectives.Count;
                 i++)
            {
                var objective =
                    Definition.objectives[i];

                var runtime =
                    ObjectiveFactory.CreateObjective(
                        ParentQuest,
                        objective,
                        StepIndex,
                        i);

                if (runtime != null)
                {
                    ObjectiveRuntimes.Add(runtime);
                }
            }
        }

        // =====================================================
        // STEP COMPLETION
        // =====================================================

        public bool IsCompleted()
        {
            foreach (var runtime
                     in ObjectiveRuntimes)
            {
                if (!runtime.IsCompleted())
                    return false;
            }

            return true;
        }
    }
}