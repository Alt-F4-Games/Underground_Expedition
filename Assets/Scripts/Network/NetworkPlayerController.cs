using Fusion;
using Network;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterController))]
public class NetworkPlayerController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerCameraPivot;
    [SerializeField] private Renderer _renderer;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Mouse")]
    [SerializeField] private float _mouseSensitivity = 0.15f;
    [SerializeField] private float _maxLookAngle = 80f;

    private NetworkCharacterController _characterController;

    private Camera _playerCamera;
    
    private float _pitch;

    
    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            _renderer.material.color = Color.yellow;

            _playerCamera = Camera.main;

            if (_playerCamera != null)
            {
                _playerCamera.transform.SetParent(_playerCameraPivot);
                _playerCamera.transform.localPosition = Vector3.zero;
                _playerCamera.transform.localRotation = Quaternion.identity;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Awake()
    {
        _characterController = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputPlayer inputPlayer)) return;
        
        //-----------------------------------------
        // ROTATION (SERVER SIMULATED)
        //-----------------------------------------

        float mouseX = inputPlayer.MouseRotation.x * _mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        //-----------------------------------------
        // CAMERA PITCH (LOCAL ONLY)
        //-----------------------------------------

        if (HasInputAuthority)
        {
            float mouseY = inputPlayer.MouseRotation.y * _mouseSensitivity;

            _pitch -= mouseY;
            _pitch = Mathf.Clamp(_pitch, -_maxLookAngle, _maxLookAngle);

            _playerCameraPivot.localRotation = Quaternion.Euler(_pitch, 0, 0);
        }

        //-----------------------------------------
        // MOVEMENT (based on player rotation)
        //-----------------------------------------

        Vector3 moveDirection =
            transform.forward * inputPlayer.MoveDirection.z +
            transform.right * inputPlayer.MoveDirection.x;

        _characterController.Move(moveDirection * _moveSpeed * Runner.DeltaTime);

        //-----------------------------------------
        // JUMP
        //-----------------------------------------

        if (inputPlayer.Buttons.IsSet(NetworkInputPlayer.JUMP_BUTTON))
        {
            if (HasStateAuthority)
                _characterController.Jump();
        }
    }
}
