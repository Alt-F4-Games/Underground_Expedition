using Events;
using Fusion;
using Network.Interaction;
using Tools.EventSystem;
using UnityEngine;

namespace Network.Quests.World
{
    public class QuestInteractable : InteractableBase
    {
        [Header("Quest")]
        [SerializeField]
        private string interactionId;

        public override void OnInteract(
            NetworkPlayerController player)
        {
            if (!HasStateAuthority)
                return;

            InteractObjectiveEvent interactObjectiveEvent = new InteractObjectiveEvent
            {
                player = player.Object.InputAuthority,
                interactionId = interactionId
            };
            
            EventController.Instance.TriggerEvent(interactObjectiveEvent);

            Debug.Log(
                $"[QuestInteractable] Interaction triggered: {interactionId}");
        }
    }
}