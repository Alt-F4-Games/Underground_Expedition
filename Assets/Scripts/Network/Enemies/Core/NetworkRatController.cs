using Fusion;
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
        
        [Tooltip("Physical size of the impact during the jump. Should be SMALLER than AttackRange.")]
        public float JumpHitboxRadius = 0.5f;
        
        // STATE FACTORY METHODS (Overrides)
        
        public override INetworkState GetAttackState()
        {
            return new NetworkChargeState(JumpChargeTime, () => 
                new NetworkJumpAttackState(JumpSpeed, JumpExtraDistance, AttackDamage, JumpHitboxRadius));
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