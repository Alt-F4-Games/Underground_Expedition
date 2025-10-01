using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerPivot;
    [SerializeField] private Camera playerCamera;

    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private string interactableTag = "Interactable";
    [SerializeField] private Transform holdPoint; // donde se "sostienen" los objetos

    private IInteractable heldObject;

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started && heldObject == null)
        {
            Ray ray = new Ray(playerPivot.position, playerPivot.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                if (hit.collider.CompareTag(interactableTag))
                {
                    if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
                    {
                        interactable.Interact(this);
                        heldObject = interactable;
                    }
                }
            }
        }
        else if (context.canceled && heldObject != null)
        {
            heldObject.Release();
            heldObject = null;
        }
    }

    public Transform GetHoldPoint()
    {
        return holdPoint;
    }
}
