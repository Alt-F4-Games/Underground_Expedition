using UnityEngine;

namespace Network.Quests.Objectives.SO
{
    [CreateAssetMenu(menuName = "Quests/Objectives/Collect Item")]
    public class CollectObjectiveSO : QuestObjectiveSO
    {
        [Header("Target")]
        public string itemId;

        private void OnValidate()
        {
            objectiveType = QuestObjectiveType.CollectItem;
        }
    }
}