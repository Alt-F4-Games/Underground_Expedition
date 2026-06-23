using System.Collections;
using Events;
using Fusion;
using Network.Spawn;
using Tools.EventSystem;
using UnityEngine;

namespace Health
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkCharacterController))]
    public class NetworkPlayerHealth : NetworkHealthSystem
    {
        [Header("Death Settings")]
        [SerializeField] private float _respawnDelay = 3f;

        private NetworkCharacterController _controller;
        private NetworkPlayerController _playerController;
        private Renderer[] _renderers;
        private Vector3 _pendingRespawnPosition;
        private PlayerDiedEvent _playerDiedEvent = new ();
        public override void Spawned()
        {
            base.Spawned();
            
            if (HasStateAuthority)
            {
                MaxHealth = 100; 
                CurrentHealth = MaxHealth;
            }

            _controller = GetComponent<NetworkCharacterController>();
            _playerController = GetComponent<NetworkPlayerController>();
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        private void OnEnable() { EventController.Instance.AddListener<PlayerStatsEvent>(IncreaseMaxHealth); }

        private void OnDisable() { EventController.Instance.RemoveListener<PlayerStatsEvent>(IncreaseMaxHealth); }
        
        // ============================================================
        // DEATH
        // ============================================================

        protected override void Death()
        {
            base.Death();

            _playerController?.PlayDeathSound();

            _playerDiedEvent.IsAlive = IsAlive;
            EventController.Instance.TriggerEvent(_playerDiedEvent);

            if (!HasStateAuthority)
                return;

            foreach (var r in _renderers)
            {
                r.enabled = false;
            }

            if (RespawnManager.Instance != null)
            {
                _pendingRespawnPosition = RespawnManager.Instance.GetCurrentSpawnPosition();
            }

            if (_controller)
            {
                _controller.enabled = false;
            }

            Runner.StartCoroutine(RespawnCoroutine());
        }
        
        public override void TakeDamage(int damage, PlayerRef playerRef)
        {
            base.TakeDamage(damage, playerRef);

            if (IsAlive)
            {
                _playerController?.PlayDamagedSound();
            }
        }

        // ============================================================
        // RESPAWN
        // ============================================================

        private IEnumerator RespawnCoroutine()
        {
            yield return new WaitForSeconds(_respawnDelay);

            if (_controller)
            {
                _controller.enabled = true;

                _controller.Teleport(_pendingRespawnPosition);
            }

            Respawn();
        }

        private void Respawn()
        {
            if (!HasStateAuthority) return;

            if (_controller)
                _controller.enabled = true;

            foreach (var r in _renderers)
            {
                r.enabled = true;
            }

            Revive();

            Debug.Log($"{gameObject.name} respawned");
        }

        private void IncreaseMaxHealth(PlayerStatsEvent evt)
        {
            if (!HasStateAuthority) return;

            Debug.Log($"{gameObject.name} max health: {MaxHealth}");
            Debug.Log($"{gameObject.name} health PLUS: {evt.MaxHealth}");
            MaxHealth += evt.MaxHealth;
            
            Debug.Log($"{gameObject.name} max health: {MaxHealth}");

            CurrentHealth = MaxHealth;
        }

        // ============================================================
        // PUBLIC API FOR STATS MANAGER
        // ============================================================
        // These methods allow the Facade (PlayerStatsManager) to safely 
        // modify health properties that have protected setters.

        public void Heal(int amount)
        {
            if (!HasStateAuthority || !IsAlive) return;
            CurrentHealth += amount;
            if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
        }

        public void AddMaxHealth(int amount)
        {
            if (!HasStateAuthority) return;
            MaxHealth += amount;
            CurrentHealth += amount; // Fill the added health
        }
    }
}