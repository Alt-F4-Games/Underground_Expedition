using System;
using Audio.Player;
using Events;
using Fusion;
using Health;
using Network;
using Skills;
using Tools.EventSystem;
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

    [Header("Camera FOV Juice")]
    [SerializeField] private float _normalFOV = 100f; // Tu FOV base (según tu captura)
    [SerializeField] private float _sprintFOV = 115f; // FOV ampliado al correr
    [SerializeField] private float _fovSpeed = 8f;     // Velocidad de la transición (Lerp)

    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 5f;

    [Header("Stamina")]
    [SerializeField] private float _maxStaminaBase = 100f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _staminaRechargeDelay = 1.5f;
    [SerializeField] private float _staminaRechargeRate = 1f;
    [SerializeField] private float _staminaDrainRate = 1f;

    [Header("Audio")]
    [SerializeField] private float walkFootstepInterval = 0.45f;
    [SerializeField] private float sprintFootstepInterval = 0.25f;
    
    [Networked] private bool IsSprinting { get; set; }
    
    [Networked] public float CurrentStamina { get; private set; }
    [Networked] public float MaxStamina { get; private set; } 
    [Networked] private float RechargeDelayTimer { get; set; }

    private NetworkCharacterController _controller;
    private NetworkPlayerHealth _health;
    
    // Reference to the Stats Manager (Facade)
    private PlayerStatsManager _statsManager;
    
    private Animator _animator;
    private EmpoweredStrikeSkill _strikeSkill;
    private IPlayerAudio _audio;    
    public static NetworkPlayerController Local { get; private set; }

    [Networked] private TickTimer StunTimer { get; set; }

    private bool _isStunnedVisual;

    private static readonly int AlphaID = Shader.PropertyToID("_noiseAlpha");

    private bool IsStunnedGameplay => !StunTimer.ExpiredOrNotRunning(Runner);

    [Networked] private float _yaw { get; set; }
    [Networked] private float _currentPitch { get; set; }
    [Networked] private float _movementSpeed { get; set; }
    [Networked] private bool IsGrounded { get; set; }
    [Networked] private bool WasGrounded { get; set; }
    [Networked] private float VerticalSpeed { get; set; }
    [Networked] private TickTimer FootstepTimer { get; set; }
    
    // Animation variables
    [Networked, OnChangedRender(nameof(OnHitReceived))] private int HitCounter { get; set; }
    [Networked, OnChangedRender(nameof(OnAttackReceived))] private int AttackCounter { get; set; }
    [Networked, OnChangedRender(nameof(OnFootstepReceived))] private int FootstepCounter { get; set; }
    [Networked, OnChangedRender(nameof(OnLandReceived))] private int LandCounter { get; set; }
    [Networked, OnChangedRender(nameof(OnAttackSoundReceived))] private int AttackSoundCounter { get; set; }
    [Networked, OnChangedRender(nameof(OnMissSoundReceived))] private int MissSoundCounter { get; set; }
    [Networked, OnChangedRender(nameof(OnDamagedReceived))] private int DamagedCounter { get; set; }
    [Networked, OnChangedRender(nameof(OnDeathReceived))] private int DeathCounter { get; set; }
    [Networked, OnChangedRender(nameof(OnEmpoweredAttackReceived))] private int EmpoweredAttackCounter { get; set; }
    [Networked, OnChangedRender(nameof(OnAttackAOEReceived))] private int AttackAOECounter { get; set; }
    
    [Networked, OnChangedRender(nameof(OnEmpoweredAttackAnimationReceived))] private int EmpoweredAttackAnimationCounter { get; set; }

    private void OnEnable() { EventController.Instance.AddListener<PlayerStatsEvent>(IncreaseMaxStamina); }

    private void OnDisable() { EventController.Instance.RemoveListener<PlayerStatsEvent>(IncreaseMaxStamina); }
    
    // ============================================================
    // SPAWN
    // ============================================================

    public override void Spawned()
    {
        _controller = GetComponent<NetworkCharacterController>();
        _health = GetComponent<NetworkPlayerHealth>();
        _audio = GetComponent<IPlayerAudio>();
        
        // Cache the Stats Manager
        _statsManager = GetComponent<PlayerStatsManager>();
        
        _cinemachineCamera = FindObjectOfType<CinemachineCamera>();
        _animator = GetComponent<Animator>();
        _strikeSkill = GetComponent<EmpoweredStrikeSkill>();
        
        if (_health != null)
        {
            _health.OnDamageTaken += OnDamageTaken;
        }
        
        if (!HasInputAuthority)
        {
            if (_cameraPivot != null) _cameraPivot.gameObject.SetActive(false);
            return;
        }
        
        Local = this;

        if (HasStateAuthority)
        {
            MaxStamina = _maxStaminaBase;
            CurrentStamina = MaxStamina;
        }

        _renderer.material.color = Color.yellow;

        // Primero instanciamos la cámara local
        SpawnCamera();

        // Buscamos la CinemachineCamera de forma segura en la escena de este cliente
        _cinemachineCamera = FindObjectOfType<CinemachineCamera>();
        
        if (_cinemachineCamera != null && _cameraPivot != null)
        {
            _cinemachineCamera.Follow = _cameraPivot;
            _cinemachineCamera.LookAt = _cameraPivot;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (_health != null)
        {
            _health.OnDamageTaken -= OnDamageTaken;
        }
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

        // ============================================================
        // JUICE: DYNAMIC FOV CHANGE (CLIENT ONLY)
        // ============================================================
        if (HasInputAuthority && _cinemachineCamera != null)
        {
            float targetFOV = (IsSprinting && _movementSpeed > 0.1f) ? _sprintFOV : _normalFOV;
            
            var lens = _cinemachineCamera.Lens;
            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, _fovSpeed * Time.deltaTime);
            _cinemachineCamera.Lens = lens;
        }

        // =========================
        // ANIMATIONS
        // =========================

        if (_animator != null)
        {
            _animator.SetFloat("movementSpeed", _movementSpeed);
            _animator.SetBool("isGrounded", IsGrounded);
            _animator.SetFloat("verticalSpeed", VerticalSpeed);
        }
    }

    // ============================================================
    // NETWORK
    // ============================================================

    public override void FixedUpdateNetwork()
    {
        if (!UI.InputManager.IsGameMode())
            return;
        
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
        HandleFootsteps(input);
        HandleJump(input);
        HandleSprint(input);
        
        IsGrounded = _controller.Grounded;
        VerticalSpeed = _controller.Velocity.y;
        HandleLanding();
    }

    private void HandleMovement(NetworkInputPlayer input)
    {
        Quaternion yawRotation = Quaternion.Euler(0, _yaw, 0);
        Vector3 moveDir = yawRotation * new Vector3(input.MoveDirection.x, 0, input.MoveDirection.z);

        // Fetch speed multipliers from the Stats Manager (default to 1f if null)
        float walkMultiplier = _statsManager != null ? _statsManager.WalkSpeedMultiplier : 1f;
        float sprintMultiplier = _statsManager != null ? _statsManager.SprintSpeedMultiplier : 1f;

        // Apply specific multipliers based on sprinting state
        _controller.maxSpeed = IsSprinting ? (_sprintSpeed * sprintMultiplier) : (_walkSpeed * walkMultiplier);

        if (IsStunnedGameplay)
        {
            _controller.Velocity = Vector3.zero;
            _movementSpeed = 0f;
            return;
        }

        _controller.Move(moveDir);

        // =========================
        // ANIMATION SPEED
        // =========================

        if (moveDir.sqrMagnitude > 0.01f)
            _movementSpeed = IsSprinting ? 1f : 0.5f;
        else
            _movementSpeed = 0f;
    }

    private void HandleFootsteps(NetworkInputPlayer input)
    {
        if (!HasStateAuthority)
            return;

        if (!_controller.Grounded)
            return;

        if (input.MoveDirection.sqrMagnitude < 0.01f)
            return;

        if (!FootstepTimer.ExpiredOrNotRunning(Runner))
            return;

        FootstepCounter++;

        float interval = IsSprinting
            ? sprintFootstepInterval
            : walkFootstepInterval;

        FootstepTimer = TickTimer.CreateFromSeconds(
            Runner,
            interval);
    }
    
    private void HandleLanding()
    {
        if (!HasStateAuthority)
            return;

        if (!WasGrounded && IsGrounded)
        {
            LandCounter++;
        }

        WasGrounded = IsGrounded;
    }
    
    private void HandleSprint(NetworkInputPlayer input)
    {
        bool wantsToSprint = input.Buttons.IsSet(NetworkInputPlayer.SPRINT_BUTTON);

        bool isMoving = input.MoveDirection.sqrMagnitude > 0.01f;

        if (wantsToSprint && isMoving && CurrentStamina > 0f)
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
    
    private void OnDamageTaken()
    {
        PlayHitAnimation();
    }
    
    private void OnHitReceived()
    {
        if (_animator == null)
            return;

        _animator.SetTrigger("Hit");
    }

    private void PlayHitAnimation()
    {
        if (!HasStateAuthority)
            return;

        HitCounter++;
    }
    private void OnAttackReceived()
    {
        if (_animator == null)
            return;

        _animator.SetTrigger("Attack");
    }

    public void PlayAttackAnimation()
    {
        if (!HasStateAuthority)
            return;

        AttackCounter++;
    }
    
    public void ModifyStamina(float amount)
    {
        if (!HasStateAuthority) return;

        CurrentStamina += amount;

        if (CurrentStamina > MaxStamina)
            CurrentStamina = MaxStamina;
        else if (CurrentStamina < 0f)
            CurrentStamina = 0f;
    }
    
    private void OnFootstepReceived() { _audio?.PlayFootstep();}
    private void OnLandReceived() { _audio?.PlayLand(); }
    private void OnAttackSoundReceived() { _audio?.PlayAttack(); }
    private void OnMissSoundReceived() { _audio?.PlayMissAttack(); }
    private void OnDamagedReceived() { _audio?.PlayDamaged(); }
    private void OnDeathReceived() { _audio?.PlayDeath(); }
    private void OnEmpoweredAttackReceived() { _audio?.PlayEmpoweredAttack(); }
    private void OnAttackAOEReceived() { _audio?.PlayAttackAOE(); }
    
    private void OnEmpoweredAttackAnimationReceived()
    {
        if (_animator == null)
            return;

        _animator.SetTrigger("Strike");
    }
    
    public void PlayEmpoweredAttackAnimation()
    {
        if (!HasStateAuthority)
            return;

        EmpoweredAttackAnimationCounter++;
    }
    
    public void PlayAttackSound()
    {
        if (!HasStateAuthority)
            return;

        AttackSoundCounter++;
    }

    public void PlayMissAttackSound()
    {
        if (!HasStateAuthority)
            return;

        MissSoundCounter++;
    }
    
    public void PlayDamagedSound()
    {
        if (!HasStateAuthority)
            return;

        DamagedCounter++;
    }

    public void PlayDeathSound()
    {
        if (!HasStateAuthority)
            return;

        DeathCounter++;
    }
    
    public void PlayEmpoweredAttackSound()
    {
        if (!HasStateAuthority)
            return;

        EmpoweredAttackCounter++;
    }
    
    public void PlayAttackAoeSound()
    {
        if (!HasStateAuthority)
            return;

        AttackAOECounter++;
    }
}