using Events;
using Network.Quests.Definitions;
using Network.Quests.Runtime;
using Quests.Objectives;

namespace Network.Quests.Objectives.Types
{
    public class ExploreObjectiveRuntime : ObjectiveRuntimeBase
    {
        public ExploreObjectiveRuntime(QuestRuntime quest, QuestObjectiveDefinition definition, int stepIndex) : base(quest, definition, stepIndex)
        {
        }

        public override void Initialize()
        {
            EventController.Instance.AddListener<ZoneDiscoveredEvent>(OnZoneDiscovered);
        }

        public override void Dispose()
        {
            EventController.Instance.RemoveListener<ZoneDiscoveredEvent>(OnZoneDiscovered);
        }

        private void OnZoneDiscovered(ZoneDiscoveredEvent evt)
        {
            if (evt.zoneId != Definition.targetId)
                return;

            CurrentAmount = 1;

            EvaluateCompletion();
        }
    }
}