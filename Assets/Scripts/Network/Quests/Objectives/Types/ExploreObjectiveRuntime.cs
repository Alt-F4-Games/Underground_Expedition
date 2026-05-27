using Events;
using Network.Quests.Definitions;
using Network.Quests.Objectives.Core;
using Network.Quests.Runtime;
using Tools.EventSystem;

namespace Network.Quests.Objectives.Types
{
    public class ExploreObjectiveRuntime
        : ObjectiveRuntimeBase
    {
        public ExploreObjectiveRuntime(
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
                .AddListener<ZoneDiscoveredEvent>(
                    OnZoneDiscovered);
        }

        public override void Dispose()
        {
            EventController.Instance
                .RemoveListener<ZoneDiscoveredEvent>(
                    OnZoneDiscovered);
        }

        private void OnZoneDiscovered(
            ZoneDiscoveredEvent evt)
        {
            if (evt.zoneId !=
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