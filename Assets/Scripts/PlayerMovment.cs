using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody _rigidBody;
    private Vector2 input;

    [SerializeField] private float upForce = 250f;
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask groundMask;
    
    private bool isGrounded;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();   
    }

    void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);
        
        Color rayColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, rayColor);
    }

    private void FixedUpdate()
    {
        _rigidBody.AddForce(new Vector3(input.x, 0f, input.y) * moveForce);
    }

    public void Jump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed && isGrounded)
        {
            _rigidBody.AddForce(Vector3.up * upForce);
            Debug.Log($"Jump {callbackContext.phase}");
        }
    }

    public void Move(InputAction.CallbackContext callbackContext)
    {
        input = callbackContext.ReadValue<Vector2>();
        Debug.Log($"Move input: {input}");
    }
}