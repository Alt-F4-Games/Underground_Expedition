using Network.Interaction;
using Network.Quests.Enums;
using UnityEngine;

namespace Network.Quests.World
{
    public class QuestInteractable : InteractableBase
    {
        [Header("Quest")]
        [SerializeField]
        private string interactionId;

        public override void OnInteract(NetworkPlayerController player)
        {
            if (!player.Object.HasInputAuthority)
                return;

            NetworkQuestManager.Local.RPC_ReportQuestEvent(
                (int)QuestObjectiveType.Interact,
                interactionId,
                1);
        }
    }
}