using UnityEngine;

namespace Network.Quests.Objectives.SO
{
    [CreateAssetMenu(menuName = "Quests/Objectives/Interact")]
    public class InteractObjectiveSO : QuestObjectiveSO
    {
        [Header("Interaction")]
        public string interactionId;

        private void OnValidate()
        {
            objectiveType = QuestObjectiveType.Interact;
            requiredAmount = 1;
        }
    }
}