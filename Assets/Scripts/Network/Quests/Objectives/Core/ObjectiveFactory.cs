using Network.Quests.Definitions;
using Network.Quests.Enums;
using Network.Quests.Objectives.Types;
using Network.Quests.Runtime;

namespace Network.Quests.Objectives.Core
{
    public static class ObjectiveFactory
    {
        public static ObjectiveRuntimeBase CreateObjective(
            QuestRuntime quest,
            QuestObjectiveDefinition definition,
            int stepIndex,
            int objectiveIndex)
        {
            return definition.questObjectiveType switch
            {
                QuestObjectiveType.KillEnemy =>
                    new KillObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex,
                        objectiveIndex),

                QuestObjectiveType.CollectItem =>
                    new CollectObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex,
                        objectiveIndex),

                QuestObjectiveType.CraftItem =>
                    new CraftObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex,
                        objectiveIndex),

                QuestObjectiveType.Interact =>
                    new InteractObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex,
                        objectiveIndex),

                QuestObjectiveType.ExploreArea =>
                    new ExploreObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex,
                        objectiveIndex),

                _ => null
            };
        }
    }
}