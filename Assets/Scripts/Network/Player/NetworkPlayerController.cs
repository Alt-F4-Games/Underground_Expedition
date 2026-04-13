using System;
using Fusion;
using Health;
﻿using System.Collections;
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
    [SerializeField] private float _walkSpeed = 5f;

    [Header("Mouse")]
    [SerializeField] private float _mouseSensitivity = 0.15f;
    [SerializeField] private float _maxLookAngle = 80f;
    
    [Header("Sprint")]
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _sprintDuration = 3f;
    [SerializeField] private float _sprintCooldown = 2f;
    
    private bool _lastSprintState; //TEST//

    [Networked] private bool IsSprinting { get; set; }
    [Networked] [HideInInspector] public float SprintTimer { get; private set; }
    [Networked] [HideInInspector] public float SprintCooldownTimer { get; private set; }
    [Networked] [HideInInspector] public bool CanSprint { get; private set; }
    
    public float SprintDuration => _sprintDuration;
    public float SprintCooldown => _sprintCooldown;
    
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
    private float _stunEndTime;
    private static readonly int AlphaID = Shader.PropertyToID("_noiseAlpha");
    
    // Getters
    private bool IsStunnedGameplay => !StunTimer.ExpiredOrNotRunning(Runner);
    
    // rotaciones
    private float _yaw;
    private float _currentPitch;

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
        if (_health != null && !_health.IsAlive)
            return;
        
        if (!GetInput(out NetworkInputPlayer input))
            return;

        HandleRotation(input);
        HandleMovement(input);
        HandleJump(input);
        HandleSprint(input);
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
        
        float targetSpeed = IsSprinting ? _sprintSpeed : _walkSpeed;
        _controller.maxSpeed = targetSpeed;
        
        if (IsStunnedGameplay)
        {
            _controller.Velocity = Vector3.zero;
        }
        else
        {
            _controller.Move(moveDir);
        }
    }

    // ============================================================
    // SPRINT
    // ============================================================

    private void HandleSprint(NetworkInputPlayer input)
    {
        bool wantsToSprint = input.Buttons.IsSet(NetworkInputPlayer.SPRINT_BUTTON);

        if (wantsToSprint && CanSprint)
            IsSprinting = true;
        else
            IsSprinting = false;

        // Detect change (no spam)
        if (IsSprinting != _lastSprintState)
        {
            _lastSprintState = IsSprinting;
        }

        // Drain stamina
        if (IsSprinting && CanSprint)
        {
            SprintTimer -= Runner.DeltaTime;
            
            if (SprintTimer <= 0f)
            {
                CanSprint = false;
                IsSprinting = false;
                SprintCooldownTimer = _sprintCooldown;
            }
        }
        else if (!CanSprint)
        {
            SprintCooldownTimer -= Runner.DeltaTime;
            
            if (SprintCooldownTimer <= 0f)
            {
                CanSprint = true;
                SprintTimer = _sprintDuration;
            }
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
        if (HasStateAuthority)
        {
            StunTimer = TickTimer.CreateFromSeconds(Runner, duration);
        }
    }

    public override void Render()
    {
        if (!Object.HasInputAuthority) return;

        _isStunnedVisual = IsStunnedGameplay;

        UpdateStunEffect();
    }
    
    private void UpdateStunEffect()
    {
        float target = _isStunnedVisual ? 1f : 0f;

        _stunLerp = Mathf.MoveTowards(_stunLerp, target, Time.deltaTime * stunFadeSpeed);
        
        stunMaterial.SetFloat(AlphaID, _stunLerp);
    }
}