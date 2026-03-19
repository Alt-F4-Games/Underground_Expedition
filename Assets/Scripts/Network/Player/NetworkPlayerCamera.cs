using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerCamera : MonoBehaviour
{
   [HideInInspector] public Transform CameraPivot;
   
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
      transform.rotation = CameraPivot.rotation;
   }
   
   public void OnLook(InputAction.CallbackContext context)
   {
      _lookInput = context.ReadValue<Vector2>();
   }
}
