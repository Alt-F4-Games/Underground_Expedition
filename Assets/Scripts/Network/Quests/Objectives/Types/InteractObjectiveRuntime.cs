using Events;
using Network.Quests.Definitions;
using Network.Quests.Runtime;
using Quests.Objectives;

namespace Network.Quests.Objectives.Types
{
    public class InteractObjectiveRuntime : ObjectiveRuntimeBase
    {
        public InteractObjectiveRuntime(QuestRuntime quest, QuestObjectiveDefinition definition, int stepIndex) : base(quest, definition, stepIndex)
        {
        }

        public override void Initialize()
        {
            EventController.Instance.AddListener<InteractObjectiveEvent>(OnInteractEvent);
        }

        public override void Dispose()
        {
            EventController.Instance.RemoveListener<InteractObjectiveEvent>( OnInteractEvent);
        }

        private void OnInteractEvent(InteractObjectiveEvent evt)
        {
            if (evt.interactionId != Definition.targetId)
                return;

            CurrentAmount++;

            EvaluateCompletion();
        }
    }
}