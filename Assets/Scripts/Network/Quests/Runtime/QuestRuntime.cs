// =====================================================
// QuestRuntime.cs
// =====================================================

using Network.Quests.Definitions;

namespace Network.Quests.Runtime
{
    public class QuestRuntime
    {
        public QuestDefinitionSO Definition { get; private set; }

        public QuestState State { get; private set; }

        public string QuestId => Definition.questId;
        
        public QuestRuntime(QuestDefinitionSO definition)
        {
            Definition = definition;

            State = new QuestState { questId = definition.questId, isCompleted = false };

            BuildState();
        }

        private void BuildState()
        {
            foreach (var objective in Definition.objectives)
            { State.objectives.Add(new QuestObjectiveState { currentAmount = 0 }); }
        }
    }
}