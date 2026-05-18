using UnityEngine;

namespace Network.Quests.Objectives.SO
{
    [CreateAssetMenu(menuName = "Quests/Objectives/Craft Item")]
    public class CraftObjectiveSO : QuestObjectiveSO
    {
        [Header("Craft")]
        public string itemId;

        private void OnValidate()
        {
            objectiveType = QuestObjectiveType.CraftItem;
        }
    }
}