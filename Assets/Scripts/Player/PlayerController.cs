using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Camera playerCamera;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Camera Settings")]
        [SerializeField] private float sensitivity = 100f;
        [SerializeField] private float clampAngle = 85f;

        [Header("Footstep Settings")]
        [SerializeField] private float footstepInterval = 0.4f; // tiempo entre sonidos

        private CharacterController controller;
        private Vector2 moveInput;
        private Vector2 lookInput;

        private float xRotation = 0f;
        private float yVelocity;
        private float footstepTimer;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerCamera != null && cameraPivot != null)
            {
                playerCamera.transform.SetParent(cameraPivot);
                playerCamera.transform.localPosition = Vector3.zero;
                playerCamera.transform.localRotation = Quaternion.identity;
            }
        }

        private void Update()
        {
            HandleCamera();
            HandleMovement();
            HandleFootsteps();
        }
    
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
    
        private void HandleMovement()
        {
            Vector3 forward = cameraPivot.forward;
            Vector3 right = cameraPivot.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;
            controller.Move(moveDir * moveSpeed * Time.deltaTime);
        
            if (controller.isGrounded && yVelocity < 0)
                yVelocity = -2f;

            yVelocity += gravity * Time.deltaTime;
            controller.Move(Vector3.up * yVelocity * Time.deltaTime);
        }

        private void HandleCamera()
        {
            float mouseX = lookInput.x * sensitivity * Time.deltaTime;
            float mouseY = lookInput.y * sensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
        
            cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
            transform.Rotate(Vector3.up * mouseX);
        }

        private void HandleFootsteps()
        {
            // Detectar si hay input de movimiento y está en el suelo
            bool isMoving = moveInput.sqrMagnitude > 0.1f;
            bool isGrounded = controller.isGrounded;

            if (isMoving && isGrounded)
            {
                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f)
                {
                    SoundManager.Instance.Play("walktest");
                    footstepTimer = footstepInterval;
                }
            }
            else
            {
                // Reinicia el timer si no está caminando
                footstepTimer = 0f;
            }
        }
    }
}