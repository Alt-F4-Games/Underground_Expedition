/*
 * PlayerController
 * This script handles first-person movement, sprinting, jumping, camera control
 * and footstep playback. It uses Unity's CharacterController and the new
 * Input System. The camera is attached to a pivot that manages vertical rotation.
 *
 * Dependencies:
 *  - CharacterController component
 *  - Unity Input System (Player Input + generated input actions)
 *  - A Camera assigned as the player's view
 *  - A pivot Transform for vertical camera rotation
 *  - SoundManager (external audio system) for footstep sounds
 */

using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Pivot transform used for vertical camera rotation.")]
        [SerializeField] private Transform cameraPivot;

        [Tooltip("Player camera that will follow the pivot.")]
        [SerializeField] private Camera playerCamera;

        [Header("Movement Settings")]
        [Tooltip("Walking speed of the player.")]
        [SerializeField] private float moveSpeed = 5f;

        [Tooltip("Running speed of the player when holding sprint.")]
        [SerializeField] private float sprintSpeed = 8f;

        [Tooltip("Jump height in Unity units.")]
        [SerializeField] private float jumpHeight = 1.5f;

        [Tooltip("Gravity force applied to the player.")]
        [SerializeField] private float gravity = -9.81f;

        [Header("Camera Settings")]
        [Tooltip("Mouse sensitivity multiplier.")]
        [SerializeField] private float sensitivity = 100f;

        [Tooltip("Maximum up/down vertical camera angle.")]
        [SerializeField] private float clampAngle = 85f;

        [Header("Footstep Settings")]
        [Tooltip("Footstep interval while walking.")]
        [SerializeField] private float walkFootstepInterval = 0.4f;

        [Tooltip("Footstep interval while sprinting.")]
        [SerializeField] private float sprintFootstepInterval = 0.25f;

        [Header("Sprint Settings")]
        [Tooltip("Maximum time (in seconds) that the player can sprint.")]
        [SerializeField] private float sprintDuration = 3f;

        [Tooltip("Cooldown time before sprint becomes available again.")]
        [SerializeField] private float sprintCooldown = 2f;

        private CharacterController controller;
        private Vector2 moveInput;
        private Vector2 lookInput;

        // Vertical camera rotation
        private float xRotation;

        // Vertical velocity for gravity and jumping
        private float yVelocity;

        // Footstep timer
        private float footstepTimer;

        // Sprinting state
        private bool isSprinting;
        private bool canSprint = true;

        // Sprint timers
        private float sprintTimer;
        private float sprintCooldownTimer;

        private void Awake()
        {
            // Cache CharacterController reference
            controller = GetComponent<CharacterController>();

            // Initialize sprint timer with full sprint duration
            sprintTimer = sprintDuration;

            // Lock cursor for FPS gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Attach camera to pivot if assigned
            if (playerCamera != null && cameraPivot != null)
            {
                playerCamera.transform.SetParent(cameraPivot, false);
                playerCamera.transform.localPosition = Vector3.zero;
                playerCamera.transform.localRotation = Quaternion.identity;
            }
        }

        private void Update()
        {
            HandleCamera();
            HandleMovement();
            UpdateSprintTimers();
            HandleFootsteps();
        }
        
        // Input System Callbacks

        public void OnMove(InputAction.CallbackContext context)
        {
            // Stores movement input (WASD/Left Stick)
            moveInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            // Stores look input (Mouse/Right Stick)
            lookInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            // Trigger jump only when pressed and grounded
            if (context.performed && controller.isGrounded)
            {
                // Standard gravity-based jump formula
                yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            // Start sprinting if allowed
            if (context.started && canSprint)
                isSprinting = true;

            // Stop sprinting when key is released
            if (context.canceled)
                isSprinting = false;
        }
        
        // Sprint Timer Logic

        private void UpdateSprintTimers()
        {
            // Drain sprint timer if sprinting
            if (isSprinting && canSprint)
            {
                sprintTimer -= Time.deltaTime;

                // Ran out of sprint stamina
                if (sprintTimer <= 0f)
                {
                    canSprint = false;
                    isSprinting = false;
                    sprintCooldownTimer = sprintCooldown; // Begin cooldown
                }
            }
            else if (!canSprint)
            {
                // Restore sprint after cooldown
                sprintCooldownTimer -= Time.deltaTime;

                if (sprintCooldownTimer <= 0f)
                {
                    canSprint = true;
                    sprintTimer = sprintDuration; // Reset stamina
                }
            }
        }
        
        // ---------------------------------------------------------
        // Movement Logic
        // ---------------------------------------------------------

        private void HandleMovement()
        {
            // Calculate forward/right movement from camera pivot
            Vector3 forward = cameraPivot.forward;
            Vector3 right = cameraPivot.right;

            // Prevent movement in slopes via vertical influence
            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            // Select running or walking speed
            float currentSpeed = (isSprinting && canSprint) ? sprintSpeed : moveSpeed;

            // Final movement direction
            Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;

            // Move horizontally
            controller.Move(moveDir * currentSpeed * Time.deltaTime);

            // Reset downward velocity when grounded
            if (controller.isGrounded && yVelocity < 0)
                yVelocity = -0.5f;

            // Apply gravity
            yVelocity += gravity * Time.deltaTime;

            // Apply vertical motion
            controller.Move(Vector3.up * yVelocity * Time.deltaTime);
        }
        
        // ---------------------------------------------------------
        // Camera Look Logic
        // ---------------------------------------------------------

        private void HandleCamera()
        {
            // Mouse deltas adjusted by sensitivity
            float mouseX = lookInput.x * sensitivity * Time.deltaTime;
            float mouseY = lookInput.y * sensitivity * Time.deltaTime;

            // Vertical rotation with clamping
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
            cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Horizontal rotation rotates the player root
            transform.Rotate(Vector3.up * mouseX);
        }
        
        // ---------------------------------------------------------
        // Footstep Sounds
        // ---------------------------------------------------------

        private void HandleFootsteps()
        {
            // Movement + grounded check
            bool isMoving = moveInput.sqrMagnitude > 0.1f;
            bool isGrounded = controller.isGrounded;

            // Stop footstep cycle if not moving
            if (!isMoving || !isGrounded)
            {
                footstepTimer = 0f;
                return;
            }

            // Pick interval depending on sprint state
            float interval = (isSprinting && canSprint) ? sprintFootstepInterval : walkFootstepInterval;

            // Countdown until next footstep
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                // Play footstep sound using external audio manager
                SoundManager.Instance.Play("walktest");

                // Reset interval
                footstepTimer = interval;
            }
        }
    }
}


