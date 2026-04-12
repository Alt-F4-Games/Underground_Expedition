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
        private Animator _animator;

        private void Awake()
        {
            _enemyHealth = GetComponent<NetworkEnemyHealth>();
            _animator  = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            _enemyHealth.OnDamageTaken += NotifyHit;
            _enemyHealth.OnDamageTaken += PlayHitAnimation;
        }

        private void OnDisable()
        {
            _enemyHealth.OnDamageTaken -= NotifyHit;
            _enemyHealth.OnDamageTaken -= PlayHitAnimation;
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
        
        protected override void HandleStateChanged()
        {
            if (_animator == null) return;

            switch (CurrentState)
            {
                case NetworkEnemyState.Patrolling:
                case NetworkEnemyState.Chasing:
                    _animator.SetBool("IsMoving", true);
                    _animator.SetBool("IsCharging", false);
                    break;

                case NetworkEnemyState.Charging:
                    _animator.SetBool("IsMoving", false);
                    _animator.SetBool("IsCharging", true);
                    break;

                case NetworkEnemyState.Exploding:
                    _animator.SetBool("IsMoving", false);
                    _animator.SetBool("IsCharging", false);
                    _animator.SetTrigger("Explode");
                    break;

                case NetworkEnemyState.Idle:
                    _animator.SetBool("IsMoving", false);
                    _animator.SetBool("IsCharging", false);
                    break;
            }
        }
        
        private void PlayHitAnimation()
        {
            if (_animator == null) return;
            _animator.SetTrigger("Hit");
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