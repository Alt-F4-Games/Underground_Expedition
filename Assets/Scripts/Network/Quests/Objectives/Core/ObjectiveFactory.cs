using Network.Quests.Definitions;
using Network.Quests.Enums;
using Network.Quests.Runtime;
using Quests.Objectives;

namespace Network.Quests.Objectives.Core
{
    public static class ObjectiveFactory
    {
        public static ObjectiveRuntimeBase CreateObjective(
            QuestRuntime quest,
            QuestObjectiveDefinition definition,
            int stepIndex)
        {
            return definition.objectiveType switch
            {
                ObjectiveType.Kill =>
                    new KillObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex),

                ObjectiveType.Collect =>
                    new CollectObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex),

                ObjectiveType.Craft =>
                    new CraftObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex),

                ObjectiveType.Interact =>
                    new InteractObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex),

                ObjectiveType.Explore =>
                    new ExploreObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex),

                _ => null
            };
        }
    }
}