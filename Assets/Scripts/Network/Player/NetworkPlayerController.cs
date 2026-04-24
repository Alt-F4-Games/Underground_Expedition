using System;
using Fusion;
using Health;
using Network;
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(NetworkCharacterController))]
public class NetworkPlayerController : NetworkBehaviour, IStunnable
{
    [Header("References")]
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private Renderer _renderer;

    [Header("Camera")]
    [SerializeField] private Camera _cameraPrefab;
    private Camera _playerCameraInstance;

    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 5f;

    [Header("Sprint")]
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _sprintDuration = 3f;

    [Header("Stamina Recharge")]
    [SerializeField] private float _staminaRechargeDelay = 1.5f;
    [SerializeField] private float _staminaRechargeRate = 1f;

    [Networked] private bool IsSprinting { get; set; }
    [Networked] public float SprintTimer { get; private set; }
    [Networked] private float RechargeDelayTimer { get; set; }

    private NetworkCharacterController _controller;
    private NetworkPlayerHealth _health;

    [Networked] private TickTimer StunTimer { get; set; }

    private bool _isStunnedVisual;

    private static readonly int AlphaID = Shader.PropertyToID("_noiseAlpha");

    private bool IsStunnedGameplay => !StunTimer.ExpiredOrNotRunning(Runner);
    public float SprintDuration => _sprintDuration;

    [Networked] private float _yaw { get; set; }
    [Networked] private float _currentPitch { get; set; }

    // ============================================================
    // SPAWN
    // ============================================================

    public override void Spawned()
    {
        _controller = GetComponent<NetworkCharacterController>();
        _health = GetComponent<NetworkPlayerHealth>();

        if (!HasInputAuthority)
        {
            _cameraPivot.gameObject.SetActive(false);
            return;
        }

        if (HasStateAuthority)
        {
            SprintTimer = _sprintDuration;
        }

        _renderer.material.color = Color.yellow;

        // ⚠️ Intentar spawnear cámara (pero no confiar solo en esto)
        SpawnCamera();
    }

    private void SpawnCamera()
    {
        if (_cameraPrefab == null)
        {
            Debug.LogError("[Player] Camera prefab not assigned!");
            return;
        }

        if (_playerCameraInstance != null) return;

        _playerCameraInstance = Instantiate(_cameraPrefab);

        Debug.Log("[Player] Camera spawned");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ============================================================
    // FIX CLAVE
    // ============================================================

    public override void Render()
    {
        // 🔥 ESTE ES EL FIX REAL
        if (HasInputAuthority && _playerCameraInstance == null)
        {
            SpawnCamera();
        }

        if (HasInputAuthority)
        {
            transform.rotation = Quaternion.Euler(0, _yaw, 0);
            _cameraPivot.localRotation = Quaternion.Euler(_currentPitch, 0, 0);

            if (_playerCameraInstance != null)
            {
                _playerCameraInstance.transform.position = _cameraPivot.position;
                _playerCameraInstance.transform.rotation = _cameraPivot.rotation;
            }
        }

        _isStunnedVisual = IsStunnedGameplay;
    }

    // ============================================================
    // NETWORK
    // ============================================================

    public override void FixedUpdateNetwork()
    {
        if (_health != null && !_health.IsAlive) return;
        if (!GetInput(out NetworkInputPlayer input)) return;

        _yaw = input.MouseRotation.x;
        _currentPitch = input.MouseRotation.y;

        HandleMovement(input);
        HandleJump(input);
        HandleSprint(input);
    }

    private void HandleMovement(NetworkInputPlayer input)
    {
        Quaternion yawRotation = Quaternion.Euler(0, _yaw, 0);
        Vector3 moveDir = yawRotation * new Vector3(input.MoveDirection.x, 0, input.MoveDirection.z);

        _controller.maxSpeed = IsSprinting ? _sprintSpeed : _walkSpeed;

        if (IsStunnedGameplay)
            _controller.Velocity = Vector3.zero;
        else
            _controller.Move(moveDir);
    }

    private void HandleSprint(NetworkInputPlayer input)
    {
        bool wantsToSprint = input.Buttons.IsSet(NetworkInputPlayer.SPRINT_BUTTON);

        if (wantsToSprint && SprintTimer > 0f)
        {
            IsSprinting = true;
            SprintTimer -= Runner.DeltaTime;
            RechargeDelayTimer = _staminaRechargeDelay;
            return;
        }

        IsSprinting = false;

        if (RechargeDelayTimer > 0f)
        {
            RechargeDelayTimer -= Runner.DeltaTime;
            return;
        }

        if (SprintTimer < _sprintDuration)
        {
            SprintTimer += Runner.DeltaTime * _staminaRechargeRate;
        }
    }

    private void HandleJump(NetworkInputPlayer input)
    {
        if (input.Buttons.IsSet(NetworkInputPlayer.JUMP_BUTTON) && HasStateAuthority)
            _controller.Jump();
    }

    public void ApplyStun(float duration)
    {
        if (HasStateAuthority)
            StunTimer = TickTimer.CreateFromSeconds(Runner, duration);
    }
}