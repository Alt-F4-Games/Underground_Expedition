using Fusion;
using UnityEngine;

namespace Network.Interaction
{
    public abstract class InteractableBase : NetworkBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [Tooltip("Text displayed on the player's screen when aiming at this object.")]
        [SerializeField] protected string _promptMessage = "Interact";
        
        [Tooltip("Time in seconds the button must be held. Set to 0 for instant interaction.")]
        [SerializeField] protected float _interactionDuration = 1.5f;
        
        // IINTERACTABLE IMPLEMENTATION

        public virtual string GetInteractPrompt() => _promptMessage;
        
        public virtual float GetInteractionDuration() => _interactionDuration;
        
        public virtual bool CanInteract(PlayerRef player)
        {
            return true;
        }
        
        // EXECUTION (Must be implemented by child classes)
        
        public abstract void OnInteract(NetworkPlayerController player);
    }
}