using Events;
using Network.Quests.Definitions;
using Network.Quests.Objectives.Core;
using Network.Quests.Runtime;
using Tools.EventSystem;

namespace Network.Quests.Objectives.Types
{
    public class KillObjectiveRuntime : ObjectiveRuntimeBase
    {
        public KillObjectiveRuntime(QuestRuntime quest, QuestObjectiveDefinition definition, int stepIndex, int objectiveIndex) : base(quest, definition, stepIndex, objectiveIndex)
        {
        }

        public override void Initialize()
        {
            EventController.Instance.AddListener<EnemyDiedEvent>(OnEnemyKilled);
        }

        public override void Dispose()
        {
            EventController.Instance.RemoveListener<EnemyDiedEvent>(OnEnemyKilled);
        }

        private void OnEnemyKilled(EnemyDiedEvent evt)
        {
            if (evt.enemyId != Definition.targetId)
                return;

            AddProgress(1);
            
            EvaluateCompletion();
        }
    }
}