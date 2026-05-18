using UnityEngine;

namespace Network.Quests.Objectives.SO
{
    [CreateAssetMenu(menuName = "Quests/Objectives/Explore Area")]
    public class ExploreObjectiveSO : QuestObjectiveSO
    {
        [Header("Zone")]
        public string zoneId;

        private void OnValidate()
        {
            objectiveType = QuestObjectiveType.ExploreArea;
            requiredAmount = 1;
        }
    }
}