using UnityEngine;
using UnityEngine.InputSystem;

// Fundamental para leer el teclado/ratón directamente

namespace Network.Cinematic
{
    public class CinematicFreeCam : MonoBehaviour
    {
        [Header("Configuración de Velocidad")]
        public float baseSpeed = 5f;
        public float currentSpeedMultiplier = 1f;
        public float speedChangeRate = 2f; // Velocidad a la que cambian las marchas con Q/E
        public float minMultiplier = 0.1f;
        public float maxMultiplier = 5f;

        [Header("Configuración de Cámara")]
        public float mouseSensitivity = 0.15f;
        private float pitch = 0f;
        private float yaw = 0f;

        private void Start()
        {
            // Limpieza visual: Apagamos la UI
            Canvas mainCanvas = FindAnyObjectByType<Canvas>(); 
            if (mainCanvas != null)
            {
                mainCanvas.gameObject.SetActive(false);
                Debug.Log("[Modo Director] Canvas desactivado.");
            }

            // Ocultar y bloquear el cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Sincronizar la rotación inicial
            Vector3 angles = transform.eulerAngles;
            pitch = angles.x;
            yaw = angles.y;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard == null || mouse == null) return;

            // 1. Modificador de Velocidad (Q para frenar, E para acelerar)
            if (keyboard.qKey.isPressed)
            {
                currentSpeedMultiplier -= speedChangeRate * Time.deltaTime;
            }
            if (keyboard.eKey.isPressed)
            {
                currentSpeedMultiplier += speedChangeRate * Time.deltaTime;
            }
            currentSpeedMultiplier = Mathf.Clamp(currentSpeedMultiplier, minMultiplier, maxMultiplier);

            // 2. Rotación de la Cámara
            Vector2 mouseDelta = mouse.delta.ReadValue();
            yaw += mouseDelta.x * mouseSensitivity;
            pitch -= mouseDelta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);

            // 3. Movimiento de Vuelo (WASD)
            float moveX = 0f;
            float moveZ = 0f;

            if (keyboard.wKey.isPressed) moveZ += 1f;
            if (keyboard.sKey.isPressed) moveZ -= 1f;
            if (keyboard.dKey.isPressed) moveX += 1f;
            if (keyboard.aKey.isPressed) moveX -= 1f;

            Vector3 moveDirection = (transform.right * moveX + transform.forward * moveZ).normalized;
            transform.position += moveDirection * (baseSpeed * currentSpeedMultiplier * Time.deltaTime);
        }
    }
}