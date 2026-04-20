using System;
using Fusion;
using Health;
using System.Collections;
using Network;
using UnityEngine;

[RequireComponent(typeof(NetworkCharacterController))]
public class NetworkPlayerController : NetworkBehaviour, IStunnable
{
    [Header("References")]
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private Renderer _renderer;

    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 5f;
    
    [Header("Sprint")]
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _sprintDuration = 3f;
    
    [Header("Stamina Recharge")]
    [SerializeField] private float _staminaRechargeDelay = 1.5f;
    [SerializeField] private float _staminaRechargeRate = 1f;
    
    private bool _lastSprintState; // TEST //

    [Networked] private bool IsSprinting { get; set; }
    [Networked] [HideInInspector] public float SprintTimer { get; private set; }
    [Networked] [HideInInspector] public bool CanSprint { get; private set; }
    [Networked] private float RechargeDelayTimer { get; set; }
   
    
    public float SprintDuration => _sprintDuration;
    
    [Header("Stun Settings")]
    [SerializeField] private Material stunMaterial;
    [SerializeField] private float stunFadeSpeed = 5f;
    private float _stunLerp;

    // Components
    private NetworkCharacterController _controller;
    private NetworkPlayerHealth _health;
    private Camera _playerCamera;

    // Variables
    [Networked] private TickTimer StunTimer { get; set; }
    private bool _isStunnedVisual;
  
    private static readonly int AlphaID = Shader.PropertyToID("_noiseAlpha");
    
    private bool IsStunnedGameplay => !StunTimer.ExpiredOrNotRunning(Runner);
    
    // Rotations
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
            CanSprint = true;
        }

        _renderer.material.color = Color.yellow;
        _playerCamera = Camera.main;

        if (_playerCamera != null)
        {
            // Unparent the camera to prevent it from inheriting physics jitter (60Hz)
            _playerCamera.transform.SetParent(null);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ============================================================
    // NETWORK SIMULATION
    // ============================================================

    public override void FixedUpdateNetwork()
    {
        if (_health != null && !_health.IsAlive) return;
        if (!GetInput(out NetworkInputPlayer input)) return;

        // Calculate rotation during the network tick
        HandleRotationLogic(input);
        HandleMovement(input);
        HandleJump(input);
        HandleSprint(input);
    }

    private void HandleRotationLogic(NetworkInputPlayer input)
    {
        // Input already comes with sensitivity and clamping applied from NetworkController
        _yaw = input.MouseRotation.x;
        _currentPitch = input.MouseRotation.y;

        // Apply physical body rotation (important for movement)
        transform.rotation = Quaternion.Euler(0, _yaw, 0);
    }

    // ============================================================
    // RENDER
    // ============================================================

    public override void Render()
    {
        // Camera smoothing for the local player
        if (HasInputAuthority)
        {
            // Apply physical rotation
            transform.rotation = Quaternion.Euler(0, _yaw, 0);
            _cameraPivot.localRotation = Quaternion.Euler(_currentPitch, 0, 0);

            // Move the unparented camera to follow the pivot with 100% smoothness
            if (_playerCamera != null)
            {
                _playerCamera.transform.position = _cameraPivot.position;
                _playerCamera.transform.rotation = _cameraPivot.rotation;
            }
        }

        // Existing visual effects
        _isStunnedVisual = IsStunnedGameplay;
        UpdateStunEffect();
    }

    // ============================================================
    // MOVEMENT & OTHERS
    // ============================================================

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
            if (SprintTimer < 0f)
                SprintTimer = 0f;
            
            RechargeDelayTimer = _staminaRechargeDelay;

            return;
        }
        IsSprinting = false;

        if (wantsToSprint && SprintTimer <= 0f)
            return;

        if (RechargeDelayTimer > 0f)
        {
            RechargeDelayTimer -= Runner.DeltaTime;
            return;
        }

        if (SprintTimer < _sprintDuration)
        {
            SprintTimer += Runner.DeltaTime * _staminaRechargeRate;

            if (SprintTimer > _sprintDuration)
                SprintTimer = _sprintDuration;
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
    
    private void UpdateStunEffect()
    {
        float target = _isStunnedVisual ? 1f : 0f;
        _stunLerp = Mathf.MoveTowards(_stunLerp, target, Time.deltaTime * stunFadeSpeed);
        stunMaterial.SetFloat(AlphaID, _stunLerp);
    }
}