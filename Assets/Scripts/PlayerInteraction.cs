using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerPivot;

    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private string interactableTag = "Interactable";

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && playerPivot != null)
        {
            Ray ray = new Ray(playerPivot.position, playerPivot.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                if (hit.collider.CompareTag(interactableTag))
                {
                    Debug.Log($"✅ Interacted with {hit.collider.gameObject.name}");
                }
                else
                {
                    Debug.Log($"Hit {hit.collider.gameObject.name}, but it is not interactable.");
                }
            }
        }
    }
}
