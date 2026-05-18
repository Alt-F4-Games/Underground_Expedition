using Network.Quests.Definitions;
using Network.Quests.Runtime;
using Quests.Objectives;

namespace Network.Quests.Objectives
{
    public class CollectObjectiveRuntime : ObjectiveRuntimeBase
    {
        public CollectObjectiveRuntime(
            QuestRuntime quest,
            QuestObjectiveDefinition definition,
            int stepIndex)
            : base(quest, definition, stepIndex)
        {
        }

        public override void Initialize()
        {
        }

        public override void Dispose()
        {
        }
    }
}