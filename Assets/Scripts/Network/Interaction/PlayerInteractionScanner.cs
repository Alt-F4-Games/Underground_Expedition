using Fusion;
using UnityEngine;

namespace Network.Interaction
{
    /// <summary>
    /// Handles local detection of interactable objects for visual feedback.
    /// This component runs only on the client with Input Authority to 
    /// trigger shaders, outlines, and UI prompts without network overhead.
    /// </summary>
    public class PlayerInteractionScanner : NetworkBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("How far the player can look to detect an object.")]
        [SerializeField] private float _detectionRange = 3f;
        
        [Tooltip("The layer assigned to interactable objects.")]
        [SerializeField] private LayerMask _interactableLayer;

        [Header("References")]
        [Tooltip("The transform from which the raycast will be fired (usually the camera pivot).")]
        [SerializeField] private Transform _rayOrigin;

        // Internal state for local feedback
        private IInteractable _currentInteractable;
        private GameObject _lastHitObject;
        
        // Referencia al jugador para pasársela a los métodos Hover
        private NetworkPlayerController _controller;

        // Public getter for UI systems to access the current prompt
        public IInteractable CurrentInteractable => _currentInteractable;

        public override void Spawned()
        {
            _controller = GetComponent<NetworkPlayerController>();
        }

        public override void Render()
        {
            // We only perform visual detection for the local player
            if (!HasInputAuthority) return;
            
            if (!UI.InputManager.IsGameMode())
            {
                ClearDetection();
                return;
            }

            ScanForInteractables();
        }
        
        // Fires a raycast from the camera pivot to find objects implementing IInteractable.
        private void ScanForInteractables()
        {
            if (_rayOrigin == null) return;

            Ray ray = new Ray(_rayOrigin.position, _rayOrigin.forward);
            
            if (Physics.Raycast(ray, out RaycastHit hit, _detectionRange, _interactableLayer))
            {
                GameObject hitObj = hit.collider.gameObject;
                
                if (hitObj != _lastHitObject)
                {
                    ClearDetection();
                    
                    IInteractable interactable = hitObj.GetComponentInParent<IInteractable>();
                    
                    if (interactable != null && interactable.CanInteract(Object.InputAuthority))
                    {
                        _currentInteractable = interactable;
                        _lastHitObject = hitObj;
                        
                        _currentInteractable.OnHoverEnter(_controller);
                    }
                }
            }
            else
            {
                // If the ray hits nothing, clear the current detection
                ClearDetection();
            }
        }

        private void ClearDetection()
        {
            if (_currentInteractable != null)
            {
                _currentInteractable.OnHoverExit(_controller);
            }

            _currentInteractable = null;
            _lastHitObject = null;
        }
        
        // ============================================================
        // EDITOR DEBUG & GIZMOS
        // ============================================================
        
        private void OnDrawGizmosSelected()
        {
            if (_rayOrigin == null) return;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(_rayOrigin.position, _rayOrigin.forward * _detectionRange);
            Gizmos.DrawWireSphere(_rayOrigin.position + _rayOrigin.forward * _detectionRange, 0.1f);
        }
    }
}