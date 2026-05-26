using Events;
using Network.Quests.Definitions;
using Network.Quests.Objectives.Core;
using Network.Quests.Runtime;
using Tools.EventSystem;

namespace Network.Quests.Objectives.Types
{
    public class CollectObjectiveRuntime : ObjectiveRuntimeBase
    {
        public CollectObjectiveRuntime(QuestRuntime quest, QuestObjectiveDefinition definition, int stepIndex, int objectiveIndex) : base(quest, definition, stepIndex, objectiveIndex)
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

            AddProgress(evt.quantity);

            EvaluateCompletion();
        }
    }
}