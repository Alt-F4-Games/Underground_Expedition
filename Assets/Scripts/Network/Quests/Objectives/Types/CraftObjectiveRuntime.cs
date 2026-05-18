using Events;
using Network.Quests.Definitions;
using Network.Quests.Runtime;
using Quests.Objectives;

namespace Network.Quests.Objectives.Types
{
    public class CraftObjectiveRuntime : ObjectiveRuntimeBase
    {
        public CraftObjectiveRuntime(QuestRuntime quest, QuestObjectiveDefinition definition, int stepIndex) : base(quest, definition, stepIndex)
        {
        }

        public override void Initialize()
        {
            EventController.Instance.AddListener<ItemCraftedEvent>(OnItemCrafted);
        }

        public override void Dispose()
        {
            EventController.Instance.RemoveListener<ItemCraftedEvent>( OnItemCrafted);
        }

        private void OnItemCrafted(ItemCraftedEvent evt)
        {
            if (evt.resultItemId != Definition.targetId)
                return;

            CurrentAmount += evt.quantity;

            EvaluateCompletion();
        }
    }
}