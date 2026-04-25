using Fusion;
using UnityEngine;

namespace Network.Interaction
{
    /// <summary>
    /// Handles the server-side logic for interacting with objects.
    /// Uses NetworkInput to determine when the player is pressing the interact button
    /// and performs Lag-Compensated raycasts to prevent cheating.
    /// </summary>
    public class NetworkPlayerInteractor : NetworkBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("Maximum distance the player can be to interact.")]
        [SerializeField] private float _interactionRange = 3f;
        
        [Tooltip("The layer assigned to interactable objects.")]
        [SerializeField] private LayerMask _interactableLayer;

        [Header("References")]
        [Tooltip("The transform from which the server will fire the raycast (usually the camera pivot).")]
        [SerializeField] private Transform _rayOrigin;

        // --- NETWORKED STATE ---
        // Synchronized to all clients so the local UI knows what is happening
        [Networked] public NetworkObject CurrentTarget { get; private set; }
        [Networked] public TickTimer InteractionTimer { get; private set; }
        [Networked] public float CurrentInteractionDuration { get; private set; }

        private NetworkPlayerController _controller;

        public override void Spawned()
        {
            _controller = GetComponent<NetworkPlayerController>();
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputPlayer input))
            {
                bool isPressingInteract = input.Buttons.IsSet(NetworkInputPlayer.INTERACT_BUTTON);
                
                if (HasStateAuthority)
                {
                    ProcessServerInteraction(isPressingInteract);
                }
            }
        }

        private void ProcessServerInteraction(bool isPressingInteract)
        {
            // If the player releases the button, cancel everything
            if (!isPressingInteract)
            {
                ResetInteraction();
                return;
            }

            // If we are not currently interacting with anything, try to find a target
            if (CurrentTarget == null)
            {
                if (_rayOrigin == null) return;
                
                Ray ray = new Ray(_rayOrigin.position, _rayOrigin.forward);
                
                // Using LagCompensation ensures the server checks exactly what the client was looking at
                if (Runner.LagCompensation.Raycast(ray.origin, ray.direction, _interactionRange, Object.InputAuthority, out var hit, _interactableLayer))
                {
                    var interactableObj = hit.GameObject.GetComponentInParent<IInteractable>();
                    var netObj = hit.GameObject.GetComponentInParent<NetworkObject>();

                    if (interactableObj != null && netObj != null && interactableObj.CanInteract(Object.InputAuthority))
                    {
                        CurrentTarget = netObj;
                        CurrentInteractionDuration = interactableObj.GetInteractionDuration();
                        
                        if (CurrentInteractionDuration > 0)
                        {
                            // Start the hold timer
                            InteractionTimer = TickTimer.CreateFromSeconds(Runner, CurrentInteractionDuration);
                        }
                        else
                        {
                            // Instant interaction (Duration = 0)
                            interactableObj.OnInteract(_controller);
                            ResetInteraction();
                        }
                    }
                }
            }
            // If we are already interacting with a target, keep updating the state
            else
            {
                if (Vector3.Distance(transform.position, CurrentTarget.transform.position) > _interactionRange + 1f)
                {
                    ResetInteraction();
                    return;
                }

                // Has the timer finished?
                if (InteractionTimer.Expired(Runner))
                {
                    var interactable = CurrentTarget.GetComponent<IInteractable>();
                    
                    // Final validation before execution
                    if (interactable != null && interactable.CanInteract(Object.InputAuthority))
                    {
                        interactable.OnInteract(_controller);
                    }
                    
                    ResetInteraction();
                }
            }
        }

        private void ResetInteraction()
        {
            CurrentTarget = null;
            InteractionTimer = default;
            CurrentInteractionDuration = 0f;
        }
        
        public float GetInteractionProgress()
        {
            if (CurrentTarget == null || CurrentInteractionDuration <= 0f || !InteractionTimer.IsRunning) 
                return 0f;

            float remaining = InteractionTimer.RemainingTime(Runner) ?? 0f;
            return Mathf.Clamp01(1f - (remaining / CurrentInteractionDuration));
        }
    }
}