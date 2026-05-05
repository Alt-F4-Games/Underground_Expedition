using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Health;
using Network.Spawn;

namespace Network.Enemies.Components
{
    [RequireComponent(typeof(SphereCollider))]
    public class DamageAura : MonoBehaviour
    {
        [Tooltip("Reference to the parent NetworkObject to ensure only the Host applies logic.")]
        [SerializeField] private NetworkObject _parentNetworkObject;
        
        [Header("Aura Settings")]
        public int Damage = 9999;

        [Tooltip("Layer(s) that the aura can damage (e.g., PlayerLayer)")]
        public LayerMask TargetLayer;

        private SphereCollider _collider;
        
        private HashSet<PlayerRespawnPoint> _corruptedPoints = new();

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;
        }

        public void UpdateRadius(float newRadius)
        {
            if (_collider != null)
            {
                transform.localScale = Vector3.one * newRadius * 2f;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_parentNetworkObject == null || !_parentNetworkObject.HasStateAuthority)
                return;

            // ============================================================
            // DAMAGE
            // ============================================================

            if ((TargetLayer.value & (1 << other.gameObject.layer)) != 0)
            {
                var health = other.GetComponentInParent<NetworkHealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(Damage);
                    Debug.Log($"[SERVER] Aura dealt {Damage} damage to {other.gameObject.name}.");
                }
            }

            // ============================================================
            // ALTAR CORRUPTION
            // ============================================================

            PlayerRespawnPoint respawnPoint =
                other.GetComponent<PlayerRespawnPoint>() ??
                other.GetComponentInParent<PlayerRespawnPoint>();

            if (respawnPoint == null)
                return;
            
            if (_corruptedPoints.Contains(respawnPoint))
                return;

            var manager = RespawnManager.Instance;
            if (manager == null)
                return;

            var current = manager.GetCurrentPoint();
            
            if (respawnPoint != current)
                return;

            Debug.Log($"[BOSS] Corrupted altar: {respawnPoint.name}");

            _corruptedPoints.Add(respawnPoint);

            manager.DeactivateLastPoint();
        }

        private void OnDrawGizmosSelected()
        {
            if (_collider != null)
            {
                Gizmos.color = new Color(0.5f, 0f, 0f, 0.5f);
                Gizmos.DrawSphere(transform.position, _collider.radius);
            }
        }
    }
}