using Fusion;
using Health;
using Network.Enemies.States;
using UnityEngine;

namespace Network.Enemies.Variants
{
    /// <summary>
    /// Specialized controller for the Rat enemy. 
    /// Inherits the swarm/snitch mechanics and implements a specific jumping attack pattern.
    /// </summary>
    public class NetworkRatController : NetworkSwarmController
    {
        [Header("Rat Jump Settings")]
        // Time the rat waits in place (telegraphing) before executing the jump attack
        public float JumpChargeTime = 1.2f; 
        
        // Speed of the physical jump displacement through the air
        public float JumpSpeed = 8f; 
        
        // Extra distance added to the player's position to make the rat jump past them if they dodge
        public float JumpExtraDistance = 2f; 
        
        // Peak height of the leap arc
        public float JumpHeight = 2f;
        
        [Tooltip("Physical size of the impact during the jump. Should be SMALLER than AttackRange.")]
        public float JumpHitboxRadius = 0.5f;
        
        // Components
        private Animator _animator;
        private NetworkEnemyHealth _enemyHealth;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _enemyHealth = GetComponent<NetworkEnemyHealth>();
        }
        
        private void OnEnable()
        {
            _enemyHealth.OnDamageTaken += PlayHitAnimation;
        }

        private void OnDisable()
        {
            _enemyHealth.OnDamageTaken -= PlayHitAnimation;
        }

        // STATE FACTORY METHODS (Overrides)
        
        public override INetworkState GetAttackState()
        {
            return new NetworkChargeState(JumpChargeTime, () => 
                new NetworkJumpAttackState(JumpSpeed, JumpExtraDistance, AttackDamage, JumpHitboxRadius, JumpHeight));
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

                case NetworkEnemyState.Jumping:
                    _animator.SetBool("IsMoving", true);
                    _animator.SetBool("IsCharging", false);
                    break;

                case NetworkEnemyState.Dead:
                    _animator.SetBool("IsMoving", false);
                    _animator.SetBool("IsCharging", false);
                    _animator.SetTrigger("Die");
                    break;

                case NetworkEnemyState.Idle:
                    _animator.SetBool("IsMoving", false);
                    _animator.SetBool("IsCharging", false);
                    break;
            }
        }

		private void PlayHitAnimation()
		{
    		if (HasStateAuthority)
    		{
        		RPC_PlayHitAnimation();
    		}
		}
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayHitAnimation()
        {
            if (_animator == null) return;
            _animator.SetTrigger("Hit");
        }
        
        // Draws debug spheres in the Unity Editor for AI ranges
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, JumpHitboxRadius);
        }
    }
}