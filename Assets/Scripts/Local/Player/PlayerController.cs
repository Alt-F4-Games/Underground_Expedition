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

        private float xRotation = 0f;
        private float yVelocity;

        private float footstepTimer;

        private bool isSprinting;
        private bool canSprint = true;

        private float sprintTimer;
        private float sprintCooldownTimer;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            // Initialize sprint timer
            sprintTimer = sprintDuration;

            // Lock cursor for FPS control
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Attach camera to pivot
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
            moveInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed && controller.isGrounded)
            {
                yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started && canSprint)
                isSprinting = true;

            if (context.canceled)
                isSprinting = false;
        }
        
        // Sprint Timer Logic

        private void UpdateSprintTimers()
        {
            // If sprinting, drain stamina
            if (isSprinting && canSprint)
            {
                sprintTimer -= Time.deltaTime;

                if (sprintTimer <= 0f)
                {
                    // Out of sprint stamina — begin cooldown
                    canSprint = false;
                    isSprinting = false;
                    sprintCooldownTimer = sprintCooldown;
                }
            }
            else if (!canSprint)
            {
                // If in cooldown, restore after cooldown time
                sprintCooldownTimer -= Time.deltaTime;

                if (sprintCooldownTimer <= 0f)
                {
                    canSprint = true;
                    sprintTimer = sprintDuration;
                }
            }
        }
        
        // Movement Logic

        private void HandleMovement()
        {
            Vector3 forward = cameraPivot.forward;
            Vector3 right = cameraPivot.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            // Select correct speed
            float currentSpeed = (isSprinting && canSprint) ? sprintSpeed : moveSpeed;

            Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;
            controller.Move(moveDir * currentSpeed * Time.deltaTime);

            if (controller.isGrounded && yVelocity < 0)
                yVelocity = -0.5f;

            yVelocity += gravity * Time.deltaTime;
            controller.Move(Vector3.up * yVelocity * Time.deltaTime);
        }
        
        // Camera Look Logic

        private void HandleCamera()
        {
            float mouseX = lookInput.x * sensitivity * Time.deltaTime;
            float mouseY = lookInput.y * sensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
            cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            transform.Rotate(Vector3.up * mouseX);
        }
        
        // Footstep Sounds

        private void HandleFootsteps()
        {
            bool isMoving = moveInput.sqrMagnitude > 0.1f;
            bool isGrounded = controller.isGrounded;

            if (!isMoving || !isGrounded)
            {
                footstepTimer = 0f;
                return;
            }

            float interval = (isSprinting && canSprint) ? sprintFootstepInterval : walkFootstepInterval;

            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                SoundManager.Instance.Play("walktest");
                footstepTimer = interval;
            }
        }
    }
}

