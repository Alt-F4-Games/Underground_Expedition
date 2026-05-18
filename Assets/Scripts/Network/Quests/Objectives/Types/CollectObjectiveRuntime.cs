using Events;
using Network.Quests.Definitions;
using Network.Quests.Runtime;
using Quests.Objectives;

namespace Network.Quests.Objectives.Types
{
    public class CollectObjectiveRuntime : ObjectiveRuntimeBase
    {
        public CollectObjectiveRuntime(QuestRuntime quest, QuestObjectiveDefinition definition, int stepIndex) : base(quest, definition, stepIndex)
        {
        }

        public override void Initialize()
        {
            EventController.Instance.AddListener<ItemCollectedEvent>(OnItemCollected);
        }

        public override void Dispose()
        {
            EventController.Instance.RemoveListener<ItemCollectedEvent>( OnItemCollected);
        }

        private void OnItemCollected(ItemCollectedEvent evt)
        {
            if (evt.itemId != Definition.targetId)
                return;

            CurrentAmount += evt.quantity;

            EvaluateCompletion();
        }
    }
}