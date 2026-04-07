using System.Collections;
using Fusion;
using Network;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterController))]
public class NetworkPlayerController : NetworkBehaviour, IStunnable
{
    [Header("References")]
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private Renderer _renderer;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Mouse")]
    [SerializeField] private float _mouseSensitivity = 0.15f;
    [SerializeField] private float _maxLookAngle = 80f;

    // Components
    private NetworkCharacterController _controller;
    private Camera _playerCamera;

    // Variables
    private bool _isStunned = false;
    
    // rotaciones
    private float _yaw;
    private float _currentPitch;

    // ============================================================
    // SPAWN
    // ============================================================

    public override void Spawned()
    {
        _controller = GetComponent<NetworkCharacterController>();

        if (!HasInputAuthority)
        {
            _cameraPivot.gameObject.SetActive(false);
            return;
        }

        _renderer.material.color = Color.yellow;

        _playerCamera = Camera.main;

        if (_playerCamera != null)
        {
            _playerCamera.transform.SetParent(_cameraPivot);
            _playerCamera.transform.localPosition = Vector3.zero;
            _playerCamera.transform.localRotation = Quaternion.identity;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ============================================================
    // NETWORK SIMULATION
    // ============================================================

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputPlayer input))
            return;

        HandleRotation(input);
        HandleMovement(input);
        HandleJump(input);
    }

    // ============================================================
    // ROTATION
    // ============================================================

    private void HandleRotation(NetworkInputPlayer input)
    {
        float mouseX = input.MouseRotation.x * _mouseSensitivity;
        float mouseY = input.MouseRotation.y * _mouseSensitivity;

        // YAW
        _yaw += mouseX;

        if (HasStateAuthority)
        {
            transform.rotation = Quaternion.Euler(0, _yaw, 0);
        }

        if (!HasInputAuthority)
            return;

        // PITCH
        _currentPitch -= mouseY;
        _currentPitch = Mathf.Clamp(_currentPitch, -_maxLookAngle, _maxLookAngle);

        _cameraPivot.localRotation = Quaternion.Euler(_currentPitch, 0, 0);
    }

    // ============================================================
    // MOVEMENT
    // ============================================================

    private void HandleMovement(NetworkInputPlayer input)
    {
        Quaternion yawRotation = Quaternion.Euler(0, _yaw, 0);

        Vector3 moveDir =
            yawRotation * new Vector3(input.MoveDirection.x, 0, input.MoveDirection.z);
        
        if (_isStunned)
        {
            _controller.Velocity = Vector3.zero;
            input.MoveDirection = Vector3.zero;
        }
        else
        {
            _controller.Move(moveDir * _moveSpeed * Runner.DeltaTime);
        }
    }

    // ============================================================
    // JUMP
    // ============================================================

    private void HandleJump(NetworkInputPlayer input)
    {
        if (input.Buttons.IsSet(NetworkInputPlayer.JUMP_BUTTON) && HasStateAuthority)
        {
            _controller.Jump();
        }
    }
    
    // ============================================================
    // STATUS EFFECT
    // ============================================================

    public void ApplyStun(float duration)
    {
        if (!_isStunned)
        {
            StartCoroutine(StunCoroutine(duration));
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        _isStunned = true;
        
        yield return new WaitForSeconds(duration);
        
        _isStunned = false;
    }
}