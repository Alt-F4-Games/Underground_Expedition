using Fusion;
using UnityEngine;

namespace Health
{
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkPlayerHealth : NetworkHealthSystem
    {
        [SerializeField] private float _respawnDelay = 5f;

        private NetworkPlayerController _controller;

        private void Awake()
        {
            _controller = GetComponent<NetworkPlayerController>();
        }

        protected override void Death()
        {
            base.Death();

            if (!HasStateAuthority) return;

            Debug.Log($"{gameObject.name} died");

            if (_controller != null)
                _controller.enabled = false;

            Runner.StartCoroutine(RespawnRoutine());
        }

        private System.Collections.IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(_respawnDelay);

            Respawn();
        }

        private void Respawn()
        {
            transform.position = Vector3.zero;

            Revive();

            if (_controller != null)
                _controller.enabled = true;

            Debug.Log($"{gameObject.name} respawned");
        }
    }
}