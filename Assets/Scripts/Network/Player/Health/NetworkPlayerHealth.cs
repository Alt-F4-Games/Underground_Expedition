using System.Collections;
using Events;
using Fusion;
using Network.Spawn;
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

        private Renderer[] _renderers;

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
            
            _playerDiedEvent.IsAlive = IsAlive;
            EventController.Instance.TriggerEvent(_playerDiedEvent);

            if (!HasStateAuthority) return;

            Debug.Log($"{gameObject.name} died");

            foreach (var r in _renderers)
            {
                r.enabled = false;
            }

            Vector3 spawnPosition = Vector3.zero;

            if (RespawnManager.Instance != null)
            {
                spawnPosition = RespawnManager.Instance.GetCurrentSpawnPosition();
            }

            if (_controller != null)
            {
                _controller.enabled = true;
                _controller.Teleport(spawnPosition);
            }

            if (_controller != null)
                _controller.enabled = false;

            Runner.StartCoroutine(RespawnCoroutine());
        }

        // ============================================================
        // RESPAWN
        // ============================================================

        private IEnumerator RespawnCoroutine()
        {
            yield return new WaitForSeconds(_respawnDelay);
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
    }
}