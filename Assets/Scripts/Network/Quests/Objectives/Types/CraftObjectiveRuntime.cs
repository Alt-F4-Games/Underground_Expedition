using Events;
using Network.Quests.Definitions;
using Network.Quests.Objectives.Core;
using Network.Quests.Runtime;
using Tools.EventSystem;

namespace Network.Quests.Objectives.Types
{
    public class CraftObjectiveRuntime
        : ObjectiveRuntimeBase
    {
        public CraftObjectiveRuntime(
            QuestRuntime quest,
            QuestObjectiveDefinition definition,
            int stepIndex,
            int objectiveIndex)
            : base(
                quest,
                definition,
                stepIndex,
                objectiveIndex)
        {
        }

        public override void Initialize()
        {
            EventController.Instance
                .AddListener<ItemCraftedEvent>(
                    OnItemCrafted);
        }

        public override void Dispose()
        {
            EventController.Instance
                .RemoveListener<ItemCraftedEvent>(
                    OnItemCrafted);
        }

        private void OnItemCrafted(
            ItemCraftedEvent evt)
        {
            if (evt.resultItemId !=
                Definition.targetId)
            {
                return;
            }

            NetworkQuestManager.Local
                .RPC_AddProgressByTarget(
                    Definition.questObjectiveType,
                    Definition.targetId,
                    evt.quantity);
        }
    }
}