using Events;
using Network.Quests.Definitions;
using Network.Quests.Objectives.Core;
using Network.Quests.Runtime;
using Tools.EventSystem;

namespace Network.Quests.Objectives.Types
{
    public class InteractObjectiveRuntime
        : ObjectiveRuntimeBase
    {
        public InteractObjectiveRuntime(
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
                .AddListener<InteractObjectiveEvent>(
                    OnInteractEvent);
        }

        public override void Dispose()
        {
            EventController.Instance
                .RemoveListener<InteractObjectiveEvent>(
                    OnInteractEvent);
        }

        private void OnInteractEvent(
            InteractObjectiveEvent evt)
        {
            if (evt.interactionId !=
                Definition.targetId)
            {
                return;
            }

            NetworkQuestManager.Local
                .RPC_AddProgressByTarget(
                    Definition.questObjectiveType,
                    Definition.targetId,
                    1);
        }
    }
}