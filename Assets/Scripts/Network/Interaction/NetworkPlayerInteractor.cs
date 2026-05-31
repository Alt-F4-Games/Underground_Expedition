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
        [SerializeField]
        private float _interactionRange = 3f;

        [Tooltip("The layer assigned to interactable objects.")]
        [SerializeField]
        private LayerMask _interactableLayer;

        [Header("References")]
        [Tooltip("The transform from which the server will fire the raycast (usually the camera pivot).")]
        [SerializeField]
        private Transform _rayOrigin;

        // --- NETWORKED STATE ---
        [Networked] public NetworkObject CurrentTarget { get; private set; }
        [Networked] public TickTimer InteractionTimer { get; private set; }
        [Networked] public float CurrentInteractionDuration { get; private set; }

        private NetworkPlayerController _controller;

        public override void Spawned() { _controller = GetComponent<NetworkPlayerController>(); }

        public override void FixedUpdateNetwork()
        {
            if (!UI.InputManager.IsGameMode())
            {
                ResetInteraction();
                return;
            }

            if (GetInput(
                    out NetworkInputPlayer input))
            {
                bool isPressingInteract =
                    input.Buttons.IsSet(
                        NetworkInputPlayer.INTERACT_BUTTON);

                if (HasStateAuthority)
                {
                    ProcessServerInteraction(
                        isPressingInteract);
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
            
            if (CurrentTarget == null || !CurrentTarget.IsValid)
            {
                if (_rayOrigin == null)
                    return;

                Ray ray = new Ray(_rayOrigin.position, _rayOrigin.forward);

                if (Physics.Raycast(ray, out RaycastHit hit, _interactionRange, _interactableLayer))
                {
                    var interactables = hit.collider.GetComponentsInParent<IInteractable>();

                    var netObj = hit.collider.GetComponentInParent<NetworkObject>();

                    if (interactables.Length > 0 && netObj != null)
                    {
                        bool canInteract = false;

                        foreach (var interactable in interactables)
                        {
                            if (interactable.CanInteract(Object.InputAuthority))
                            {
                                canInteract = true;
                                break;
                            }
                        }

                        if (!canInteract)
                            return;

                        CurrentTarget = netObj;

                        CurrentInteractionDuration = interactables[0].GetInteractionDuration();

                        if (CurrentInteractionDuration > 0f)
                        {
                            InteractionTimer = TickTimer.CreateFromSeconds(Runner, CurrentInteractionDuration);
                        }
                        else
                        {
                            ExecuteInteractions(netObj);

                            ResetInteraction();
                        }
                    }
                }
            }
            else
            {
                if (_rayOrigin == null)
                    return;

                if (Vector3.Distance(_rayOrigin.position, CurrentTarget.transform.position) > _interactionRange + 1.5f)
                {
                    ResetInteraction();
                    return;
                }

                if (InteractionTimer.Expired(Runner))
                {
                    ExecuteInteractions(CurrentTarget);

                    ResetInteraction();
                }
            }
        }

        private void ExecuteInteractions(NetworkObject target)
        {
            if (target == null)
                return;

            var interactables = target.GetComponents<IInteractable>();

            foreach (var interactable in interactables)
            {
                if (!interactable.CanInteract(Object.InputAuthority))
                {
                    continue;
                }

                interactable.OnInteract(_controller);
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
            if (!CurrentTarget ||
                !CurrentTarget.IsValid ||
                CurrentInteractionDuration <= 0f ||
                !InteractionTimer.IsRunning)
            {
                return 0f;
            }

            float remaining = InteractionTimer.RemainingTime(Runner) ?? 0f;

            return Mathf.Clamp01(1f - (remaining / CurrentInteractionDuration));
        }
    }
}