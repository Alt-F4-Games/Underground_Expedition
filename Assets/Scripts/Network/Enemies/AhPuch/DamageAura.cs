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

        // ==========================================
        // NUEVO: Configuraciones de Progresión
        // ==========================================
        [Header("Aura Progression")]
        [Tooltip("Curva para suavizar el crecimiento/decrecimiento del aura.")]
        public AnimationCurve GrowthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Tooltip("Velocidad base a la que el aura alcanza su nuevo tamaño.")]
        public float GrowthSpeed = 2f;

        private float _currentVisualRadius;
        private float _targetRadius;
        private float _startRadius;
        private float _transitionProgress = 1f;
        // ==========================================

        private SphereCollider _collider;
        private HashSet<PlayerRespawnPoint> _corruptedPoints = new();

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;
        }

        // Modificamos la firma para recibir el deltaTime del Runner
        public void UpdateRadius(float newRadius, float deltaTime)
        {
            if (_collider == null) return;

            // Si el radio objetivo cambia, iniciamos una nueva transición
            if (Mathf.Abs(_targetRadius - newRadius) > 0.001f)
            {
                // Si es la primera vez (radio actual es 0), seteamos el inicio ahí o hacemos snap. 
                // Lo dejamos animar desde el tamaño actual para un efecto visual fluido.
                _startRadius = _currentVisualRadius; 
                _targetRadius = newRadius;
                _transitionProgress = 0f;
            }

            // Calculamos la interpolación progresiva si estamos en transición
            if (_transitionProgress < 1f)
            {
                _transitionProgress += deltaTime * GrowthSpeed;
                if (_transitionProgress > 1f) _transitionProgress = 1f; // Tope de seguridad

                float curveValue = GrowthCurve.Evaluate(_transitionProgress);
                _currentVisualRadius = Mathf.Lerp(_startRadius, _targetRadius, curveValue);
            }
            else
            {
                _currentVisualRadius = _targetRadius;
            }

            // Aplicamos la escala real
            transform.localScale = Vector3.one * _currentVisualRadius * 2f;
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