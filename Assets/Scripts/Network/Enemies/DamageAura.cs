using Fusion;
using UnityEngine;
using Health;

namespace Network.Enemies.Components
{
    [RequireComponent(typeof(SphereCollider))]
    public class DamageAura : MonoBehaviour
    {
        [Tooltip("Referencia al NetworkObject padre para asegurar que solo el Host aplique daño.")]
        [SerializeField] private NetworkObject _parentNetworkObject;
        
        [Header("Aura Settings")]
        public int Damage = 9999; // 
        public float BaseRadius = 3f; // Radio inicial 

        private SphereCollider _collider;

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;
            
            // Setear el radio inicial
            UpdateRadius(BaseRadius);
        }
        
        // Actualiza el radio del collider.
        public void UpdateRadius(float newRadius)
        {
            if (_collider != null)
            {
                _collider.radius = newRadius;
            }
            
        }

        private void OnTriggerEnter(Collider other)
        {
            // Solo el Host tiene autoridad para aplicar daño
            if (_parentNetworkObject == null || !_parentNetworkObject.HasStateAuthority) return;

            var health = other.GetComponentInParent<NetworkHealthSystem>();
            if (health != null)
            {
                health.TakeDamage(Damage);
                Debug.Log($"[SERVER] Aura hizo {Damage} de daño a {other.gameObject.name}.");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.5f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(transform.position, BaseRadius);
        }
    }
}