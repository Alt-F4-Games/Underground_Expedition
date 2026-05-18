using UnityEngine;

namespace Network.Quests.Objectives.SO
{
    [CreateAssetMenu(menuName = "Quests/Objectives/Kill Enemy")]
    public class KillObjectiveSO : QuestObjectiveSO
    {
        [Header("Target")]
        public string enemyId;

        private void OnValidate()
        {
            objectiveType = QuestObjectiveType.KillEnemy;
        }
    }
}