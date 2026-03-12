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
    [SerializeField] private float _cameraSmooth = 15f;

    private NetworkCharacterController _characterController;
    private Camera _playerCamera;
    
    private float _pitch;
    private float _targetPitch;
    private float _targetYaw;

    
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

    // ============================================================
    // Network Simulation
    // ============================================================

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputPlayer inputPlayer))
            return;

        HandleRotation(inputPlayer);
        HandleMovement(inputPlayer);
        HandleJump(inputPlayer);
    }

    // ============================================================
    // Rotation Logic
    // ============================================================

    private void HandleRotation(NetworkInputPlayer inputPlayer)
    {
        float mouseX = inputPlayer.MouseRotation.x * _mouseSensitivity;
        float mouseY = inputPlayer.MouseRotation.y * _mouseSensitivity;

        // Accumulate target yaw (horizontal rotation)
        _targetYaw += mouseX;

        // Create target rotation
        Quaternion targetRotation = Quaternion.Euler(0, _targetYaw, 0);

        // Smoothly interpolate toward target rotation
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            _cameraSmooth * Runner.DeltaTime
        );

        // Vertical camera rotation (local camera only)
        if (HasInputAuthority && _playerCameraPivot != null)
        {
            _targetPitch -= mouseY;
            _targetPitch = Mathf.Clamp(_targetPitch, -85f, 85f);

            _pitch = Mathf.Lerp(_pitch, _targetPitch, _cameraSmooth * Runner.DeltaTime);

            _playerCameraPivot.localRotation = Quaternion.Euler(_pitch, 0, 0);
        }
    }

    // ============================================================
    // Movement Logic
    // ============================================================

    
    private void HandleMovement(NetworkInputPlayer inputPlayer)
    {
        Vector3 input = inputPlayer.MoveDirection;

        Vector3 moveDirection =
            transform.forward * input.z +
            transform.right * input.x;

        moveDirection.y = 0f;

        _characterController.Move(moveDirection.normalized * _moveSpeed * Runner.DeltaTime);
    }

    // ============================================================
    // Jump Logic
    // ============================================================

    private void HandleJump(NetworkInputPlayer inputPlayer)
    {
        if (inputPlayer.Buttons.IsSet(NetworkInputPlayer.JUMP_BUTTON) && HasStateAuthority)
        {
            _characterController.Jump();
        }
    }
}
