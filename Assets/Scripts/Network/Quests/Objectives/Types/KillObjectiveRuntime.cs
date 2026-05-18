using Events;
using Network.Quests.Definitions;
using Network.Quests.Runtime;
using Quests.Objectives;

namespace Network.Quests.Objectives.Types
{
    public class KillObjectiveRuntime : ObjectiveRuntimeBase
    {
        public KillObjectiveRuntime(QuestRuntime quest, QuestObjectiveDefinition definition, int stepIndex) : base(quest, definition, stepIndex)
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

            CurrentAmount++;

            EvaluateCompletion();
        }
    }
}