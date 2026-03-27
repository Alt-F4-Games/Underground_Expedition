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

        private NetworkCharacterController _controller;

        private Renderer[] _renderers;

        public override void Spawned()
        {
            base.Spawned();

            _controller = GetComponent<NetworkCharacterController>();

            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        // ============================================================
        // DEATH
        // ============================================================

        protected override void Death()
        {
            base.Death();

            if (!HasStateAuthority) return;

            Debug.Log($"{gameObject.name} died");

            // 🔻 Ocultar visuales
            foreach (var r in _renderers)
            {
                r.enabled = false;
            }

            // Obtener posición de respawn
            Vector3 spawnPosition = Vector3.zero;

            if (RespawnManager.Instance != null)
            {
                spawnPosition = RespawnManager.Instance.GetCurrentSpawnPosition();
            }

            // 🔥 Mover INMEDIATAMENTE al altar
            if (_controller != null)
            {
                _controller.enabled = true;
                _controller.Teleport(spawnPosition);
            }

            // 🚫 Desactivar movimiento después de mover
            if (_controller != null)
                _controller.enabled = false;

            // ⏳ Esperar en el altar
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

            // Reactivar controller
            if (_controller != null)
                _controller.enabled = true;

            // 🔺 Mostrar visuales
            foreach (var r in _renderers)
            {
                r.enabled = true;
            }

            // Revivir
            Revive();

            Debug.Log($"{gameObject.name} respawned");
        }
    
        
    }
}