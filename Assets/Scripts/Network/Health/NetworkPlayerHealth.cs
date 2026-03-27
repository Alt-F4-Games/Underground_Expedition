using System.Collections;
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

        [Header("References")]
        [SerializeField] private Renderer _renderer;

        private NetworkCharacterController _controller;

        public override void Spawned()
        {
            base.Spawned();

            _controller = GetComponent<NetworkCharacterController>();
        }

        protected override void Death()
        {
            base.Death();

            if (!HasStateAuthority) return;

            Debug.Log($"{gameObject.name} died");

            // Desactivar visual
            if (_renderer != null)
                _renderer.enabled = false;

            // Desactivar movimiento
            if (_controller != null)
                _controller.enabled = false;

            // Respawn con delay
            Runner.StartCoroutine(RespawnCoroutine());
        }

        private IEnumerator RespawnCoroutine()
        {
            yield return new WaitForSeconds(_respawnDelay);

            Respawn();
        }

        private void Respawn()
        {
            if (!HasStateAuthority) return;

            Vector3 spawnPosition = Vector3.zero;
            
            if (RespawnManager.Instance != null)
            {
                spawnPosition = RespawnManager.Instance.GetCurrentSpawnPosition();
            }

            if (_controller != null)
                _controller.enabled = false;

            transform.position = spawnPosition;

            if (_controller != null)
                _controller.enabled = true;

            if (_renderer != null)
                _renderer.enabled = true;

            Revive();

            Debug.Log($"{gameObject.name} respawned at {spawnPosition}");
        }
    
        
    }
}