using System;
using Events;
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
    private CinemachineCamera _cinemachineCamera;

    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 5f;

    [Header("Stamina")]
    [SerializeField] private float _maxStaminaBase = 100f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _staminaRechargeDelay = 1.5f;
    [SerializeField] private float _staminaRechargeRate = 1f;
    [SerializeField] private float _staminaDrainRate = 1f;

    [Networked] private bool IsSprinting { get; set; }
    [Networked] public float CurrentStamina { get; private set; }
    [Networked]  public float MaxStamina { get; private set; } 
    [Networked] private float RechargeDelayTimer { get; set; }

    private NetworkCharacterController _controller;
    private NetworkPlayerHealth _health;

    [Networked] private TickTimer StunTimer { get; set; }

    private bool _isStunnedVisual;

    private static readonly int AlphaID = Shader.PropertyToID("_noiseAlpha");

    private bool IsStunnedGameplay => !StunTimer.ExpiredOrNotRunning(Runner);

    [Networked] private float _yaw { get; set; }
    [Networked] private float _currentPitch { get; set; }

    private void OnEnable() { EventController.Instance.AddListener<PlayerStatsEvent>(IncreaseMaxStamina); }

    private void OnDisable() { EventController.Instance.RemoveListener<PlayerStatsEvent>(IncreaseMaxStamina); }
    
    // ============================================================
    // SPAWN
    // ============================================================

    public override void Spawned()
    {
        _controller = GetComponent<NetworkCharacterController>();
        _health = GetComponent<NetworkPlayerHealth>();
        _cinemachineCamera = FindObjectOfType<CinemachineCamera>();

        if (!HasInputAuthority)
        {
            if (_cameraPivot != null) _cameraPivot.gameObject.SetActive(false);
            return;
        }

        if (HasStateAuthority)
        {
            MaxStamina = _maxStaminaBase;
            CurrentStamina = MaxStamina;
        }
        
        if (_cinemachineCamera != null)
        {
            _cinemachineCamera.Follow = _cameraPivot;
            _cinemachineCamera.LookAt = _cameraPivot;
        }

        _renderer.material.color = Color.yellow;

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
    // RENDER (VISUALS - CLIENT ONLY)
    // ============================================================

    public override void Render()
    {
        if (HasInputAuthority && _playerCameraInstance == null)
        {
            SpawnCamera();
        }
        
        transform.rotation = Quaternion.Euler(0, _yaw, 0);
        if (_cameraPivot != null)
        {
            _cameraPivot.localRotation = Quaternion.Euler(_currentPitch, 0, 0);
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
        
        transform.rotation = Quaternion.Euler(0, _yaw, 0);
        if (_cameraPivot != null)
        {
            _cameraPivot.localRotation = Quaternion.Euler(_currentPitch, 0, 0);
        }

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

        if (wantsToSprint && CurrentStamina > 0f)
        {
            IsSprinting = true;

            CurrentStamina -= _staminaDrainRate * Runner.DeltaTime;
            if (CurrentStamina < 0f)
                CurrentStamina = 0f;

            RechargeDelayTimer = _staminaRechargeDelay;

            return;
        }

        IsSprinting = false;

        if (wantsToSprint && CurrentStamina <= 0f)
            return;

        // =========================
        // DELAY
        // =========================
        if (RechargeDelayTimer > 0f)
        {
            RechargeDelayTimer -= Runner.DeltaTime;
            return;
        }

        // =========================
        // REGEN
        // =========================
        if (CurrentStamina < MaxStamina)
        {
            CurrentStamina += _staminaRechargeRate * Runner.DeltaTime;

            if (CurrentStamina > MaxStamina)
                CurrentStamina = MaxStamina;
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
    
    public void IncreaseMaxStamina(PlayerStatsEvent evt)
    {
        if (!HasStateAuthority) return;

        MaxStamina += evt.MaxStamina;
        CurrentStamina = MaxStamina;
    }
}