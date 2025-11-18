using Player;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableObject : MonoBehaviour, IHoldable
{
    private Rigidbody rb;
    private bool isHeld;
    private Transform holdPoint;

    [SerializeField] private float followSpeed = 15f;
    [SerializeField] private float rotateSpeed = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Interact(PlayerInteraction interactor)
    {
        isHeld = true;
        holdPoint = interactor.GetHoldPoint();
        rb.useGravity = false;
        rb.linearDamping = 10f; 
    }

    public void Release()
    {
        isHeld = false;
        rb.useGravity = true;
        rb.linearDamping = 0f;
        holdPoint = null;
    }

    private void FixedUpdate()
    {
        if (isHeld && holdPoint != null)
        {
            Vector3 targetPosition = holdPoint.position;
            Vector3 forceDirection = targetPosition - transform.position;
            rb.linearVelocity = forceDirection * followSpeed;
            
            Quaternion targetRotation = holdPoint.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeed));
        }
    }
}