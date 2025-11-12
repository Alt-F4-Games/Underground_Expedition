using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerCamera : MonoBehaviour
{
   [HideInInspector] public Transform CameraPivot;
   [SerializeField] private float MouseSensitivity;
   
   private float YRotation;
   private float XRotation;
   private Vector2 _lookInput;

   
   void LateUpdate()
   {
      if (CameraPivot == null)
      {
         return;
      }

      transform.position = CameraPivot.position;

      float mouseX = _lookInput.x * MouseSensitivity * Time.deltaTime;
      float mouseY = _lookInput.y * MouseSensitivity * Time.deltaTime;

      YRotation -= mouseY;
      YRotation = Mathf.Clamp(YRotation, -90f, 90f);

      XRotation += mouseX;

      transform.localRotation = Quaternion.Euler(YRotation, XRotation, 0);
   }
   
   public void OnLook(InputAction.CallbackContext context)
   {
      _lookInput = context.ReadValue<Vector2>();
   }
}
