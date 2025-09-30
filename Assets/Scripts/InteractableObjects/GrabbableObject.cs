using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableObject : MonoBehaviour, IInteractable
{
    private Rigidbody rb;
    private bool isHeld;
    private Transform holdPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Interact(PlayerInteraction interactor)
    {
        isHeld = true;
        holdPoint = interactor.GetHoldPoint();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void Release()
    {
        isHeld = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        holdPoint = null;
    }

    private void FixedUpdate()
    {
        if (isHeld && holdPoint != null)
        {
            Vector3 targetPosition = holdPoint.position;
            Vector3 forceDirection = targetPosition - transform.position;
            rb.linearVelocity = forceDirection * 10f; // mueve el objeto hacia el holdPoint
        }
    }
}