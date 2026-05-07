using Fusion;
using UnityEngine;

namespace Network.Interaction
{
    /// <summary>
    /// Handles the server-side logic for interacting with objects.
    /// Uses standard server physics to validate distance and line of sight.
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
            if (!UI.InputManager.IsGameMode())
            {
                ResetInteraction();
                return;
            }
            
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
            if (!isPressingInteract)
            {
                ResetInteraction();
                return;
            }

            // If we are not currently interacting with anything, try to find a target
            if (CurrentTarget == null || !CurrentTarget.IsValid)
            {
                if (_rayOrigin == null) return;

                Ray ray = new Ray(_rayOrigin.position, _rayOrigin.forward);
                
                if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactableLayer))
                {
                    var interactableObj = hit.collider.GetComponentInParent<IInteractable>();
                    var netObj = hit.collider.GetComponentInParent<NetworkObject>();

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
                            // Instant interaction
                            interactableObj.OnInteract(_controller);
                            ResetInteraction();
                        }
                    }
                }
            }
            // If we are already interacting with a target, keep updating the state
            else
            {
                if (_rayOrigin == null) return;
                
                if (Vector3.Distance(_rayOrigin.position, CurrentTarget.transform.position) > _interactionRange + 1.5f)
                {
                    ResetInteraction();
                    return;
                }

                if (InteractionTimer.Expired(Runner))
                {
                    var interactable = CurrentTarget.GetComponent<IInteractable>();
                    
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
            if (CurrentTarget == null || !CurrentTarget.IsValid || CurrentInteractionDuration <= 0f || !InteractionTimer.IsRunning) 
                return 0f;

            float remaining = InteractionTimer.RemainingTime(Runner) ?? 0f;
            return Mathf.Clamp01(1f - (remaining / CurrentInteractionDuration));
        }
    }
}