using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Camera playerCamera;

        [Header("Settings")]
        [SerializeField] private float sensitivity = 100f;
        [SerializeField] private float clampAngle = 90f;

        private Vector2 lookInput;
        private float xRotation = 0f;

        private void Start()
        {
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
            float mouseX = lookInput.x * sensitivity * Time.deltaTime;
            float mouseY = lookInput.y * sensitivity * Time.deltaTime;
        
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
            cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
            playerBody.Rotate(Vector3.up * mouseX);
        }
    
        public void OnLook(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }
    }
}