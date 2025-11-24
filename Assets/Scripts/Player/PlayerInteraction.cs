/*
 * PlayerInteraction
 * -----------------
 * Handles player interactions with world objects using raycasts.
 * Supports:
 *  - Interacting with objects implementing IInteractable
 *  - Picking up holdable objects (IHoldable) and releasing them
 *  - Uses the player's camera forward direction for interaction
 *
 * Dependencies:
 *  - Unity Input System (OnInteract callback)
 *  - A Camera assigned to playerCamera
 *  - Objects in the world implementing IInteractable / IHoldable
 *  - A hold point Transform for carried objects
 */

using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference used to determine player's forward direction for interactions.")]
        [SerializeField] private Transform playerPivot;

        [Tooltip("Camera used to cast interaction ray forward.")]
        [SerializeField] private Camera playerCamera;

        [Header("Interaction Settings")]
        [Tooltip("Maximum distance at which the player can interact with objects.")]
        [SerializeField] private float interactDistance = 3f;

        [Tooltip("Tag used to filter interactable objects in the world.")]
        [SerializeField] private string interactableTag = "Interactable";

        [Tooltip("Point where holdable objects will be attached when picked up.")]
        [SerializeField] private Transform holdPoint;

        // Currently held object (if any)
        private IHoldable heldObject;

        // Input callback for interact action
        public void OnInteract(InputAction.CallbackContext context)
        {
            // Interaction performed while not holding an object
            if (context.performed && heldObject == null)
            {
                // Create a ray from camera forward
                Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

                // Check for hit
                if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
                {
                    // Filter by tag
                    if (hit.collider.CompareTag(interactableTag))
                    {
                        // Attempt to retrieve an IInteractable component
                        if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                        {
                            interactable.Interact(this);
                        }
                    }
                }
            }
            // Interaction canceled while holding an object → release it
            else if (context.canceled && heldObject != null)
            {
                heldObject.Release();
                heldObject = null;
            }
        }

        // Returns the Transform where holdable objects should be positioned
        public Transform GetHoldPoint()
        {
            return holdPoint;
        }
    }
}
