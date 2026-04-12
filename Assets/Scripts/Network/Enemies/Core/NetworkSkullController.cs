using Health;
using Network.Enemies.States;
using UnityEngine;

namespace Network.Enemies
{
    public class NetworkSkullController : NetworkEnemyController
    {
        [Header("Explosion Settings")]
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private int explosionDamage = 30;
        [SerializeField] private float explosionStunTime = 2f;

        [Header("Charge Settings")]
        [SerializeField] private float chargeTime = 2f;

        [Header("Floating Settings")]
        [SerializeField] private float floatHeight = 2.5f;
        [SerializeField] private float floatAmplitude = 0.25f;
        [SerializeField] private float floatSpeed = 2f;

        // Variables
        private float _baseOffset;
        private bool _wasHitDuringCharge;
        
        // Getters
        public bool WasHitDuringCharge => _wasHitDuringCharge;
        
        // Components
        private NetworkEnemyHealth _enemyHealth;

        private void Awake()
        {
            _enemyHealth = GetComponent<NetworkEnemyHealth>();
        }

        private void OnEnable()
        {
            _enemyHealth.OnDamageTaken += NotifyHit;
        }

        private void OnDisable()
        {
            _enemyHealth.OnDamageTaken -= NotifyHit;
        }

        public override void Spawned()
        {
            base.Spawned();

            Agent.updateUpAxis = false;
            Agent.updateRotation = true;

            _baseOffset = floatHeight;
            Agent.baseOffset = floatHeight;
        }

        public override void FixedUpdateNetwork()
        {
            if (Object == null || !Object.IsValid)
                return;

            base.FixedUpdateNetwork();
            
            if (Agent == null || !Agent.isActiveAndEnabled)
                return;
            
            if (!Agent.isOnNavMesh)
                return;
            
            Agent.baseOffset = _baseOffset + Mathf.Sin(Runner.SimulationTime * floatSpeed) * floatAmplitude;
        }
        
        public override INetworkState GetAttackState()
        {
            return new NetworkResettableChargeState(
                chargeTime,
                () => WasHitDuringCharge,   // condition
                ResetHitFlag,
                () => new NetworkExplodeState(explosionRadius, explosionDamage, explosionStunTime)
            );
        }
        
        private void NotifyHit()
        {
            _wasHitDuringCharge = true;
        }
        
        public void ResetHitFlag()
        {
            _wasHitDuringCharge = false;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Explosion radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);

            // Attack trigger range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
        }
    }
}