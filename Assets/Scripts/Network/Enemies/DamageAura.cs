using Fusion;
using UnityEngine;
using Health;

namespace Network.Enemies.Components
{
    [RequireComponent(typeof(SphereCollider))]
    public class DamageAura : MonoBehaviour
    {
        [Tooltip("Reference to the parent NetworkObject to ensure only the Host applies damage.")]
        [SerializeField] private NetworkObject _parentNetworkObject;
        
        [Header("Aura Settings")]
        public int Damage = 9999; 
        
        [Tooltip("Layer(s) that the aura can damage (e.g., PlayerLayer)")]
        public LayerMask TargetLayer; 

        private SphereCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;
            // Initial radius is no longer set here; the Boss Controller will inject the AttackRange.
        }
        
        // Updates the collider radius dynamically
        public void UpdateRadius(float newRadius)
        {
            if (_collider != null)
            {
                _collider.radius = newRadius;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Only the Host has authority to apply damage
            if (_parentNetworkObject == null || !_parentNetworkObject.HasStateAuthority) return;

            // Filter to prevent friendly fire using the LayerMask
            if ((TargetLayer.value & (1 << other.gameObject.layer)) == 0) return;

            var health = other.GetComponentInParent<NetworkHealthSystem>();
            if (health != null)
            {
                health.TakeDamage(Damage);
                Debug.Log($"[SERVER] Aura dealt {Damage} damage to {other.gameObject.name}.");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_collider != null)
            {
                Gizmos.color = new Color(0.5f, 0f, 0f, 0.5f); // Semi-transparent red for danger
                Gizmos.DrawSphere(transform.position, _collider.radius);
            }
        }
    }
}