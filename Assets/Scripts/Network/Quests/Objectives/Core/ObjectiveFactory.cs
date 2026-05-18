using Network.Quests.Definitions;
using Network.Quests.Enums;
using Network.Quests.Objectives.Types;
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
            return definition.questObjectiveType switch
            {
                Enums.QuestObjectiveType.KillEnemy =>
                    new KillObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex),

                Enums.QuestObjectiveType.CollectItem =>
                    new CollectObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex),

                Enums.QuestObjectiveType.CraftItem =>
                    new CraftObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex),

                Enums.QuestObjectiveType.Interact =>
                    new InteractObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex),

                Enums.QuestObjectiveType.ExploreArea =>
                    new ExploreObjectiveRuntime(
                        quest,
                        definition,
                        stepIndex),

                _ => null
            };
        }
    }
}