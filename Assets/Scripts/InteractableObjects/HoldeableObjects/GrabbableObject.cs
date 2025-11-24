/*
 * GrabbableObject
 * ----------------
 * Implements a physics-based holdable object using Rigidbody.
 * Allows the player to pick up the object (via IHoldable) and have it follow
 * a target "hold point" in front of the player. Movement uses Rigidbody velocity
 * for smooth physics-based motion, and rotation uses interpolation with Slerp.
 *
 * Dependencies:
 *  - Rigidbody component (required by [RequireComponent])
 *  - PlayerInteraction must provide a valid hold point Transform
 *  - Implements IHoldable → can be grabbed and released
 */

using Player;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableObject : MonoBehaviour, IHoldable
{
    private Rigidbody rb;

    // Whether the object is currently being held
    private bool isHeld;

    // Target point to follow when held
    private Transform holdPoint;

    [Header("Follow Settings")]
    [Tooltip("Speed at which the object moves toward the hold point.")]
    [SerializeField] private float followSpeed = 15f;

    [Tooltip("Speed of rotation interpolation toward the hold point's rotation.")]
    [SerializeField] private float rotateSpeed = 10f;

    private void Awake()
    {
        // Cache Rigidbody reference
        rb = GetComponent<Rigidbody>();
    }

    public void Interact(PlayerInteraction interactor)
    {
        // Mark as held, and assign follow target
        isHeld = true;
        holdPoint = interactor.GetHoldPoint();

        // Disable gravity so object floats at hold point
        rb.useGravity = false;

        // Add damping to reduce jitter while following
        rb.linearDamping = 10f;
    }

    public void Release()
    {
        // Stop holding the object
        isHeld = false;

        // Restore gravity and normal physics
        rb.useGravity = true;
        rb.linearDamping = 0f;

        holdPoint = null;
    }

    private void FixedUpdate()
    {
        // Only follow if currently held and a valid target exists
        if (isHeld && holdPoint != null)
        {
            // Move toward the hold point using linear velocity
            Vector3 targetPosition = holdPoint.position;
            Vector3 forceDirection = targetPosition - transform.position;
            rb.linearVelocity = forceDirection * followSpeed;

            // Smoothly rotate to match the hold point's rotation
            Quaternion targetRotation = holdPoint.rotation;
            rb.MoveRotation(
                Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotateSpeed)
            );
        }
    }
}
