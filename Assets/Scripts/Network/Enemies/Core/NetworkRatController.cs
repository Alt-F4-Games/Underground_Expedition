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
        
        // STATE FACTORY METHODS (Overrides)
        
        public override INetworkState GetAttackState()
        {
            // Instanciamos el estado genérico pasándole el tiempo de la rata.
            // Temporalmente, le decimos que vuelva a GetChaseState() al terminar, 
            // hasta que creemos el NetworkJumpState.
            return new NetworkChargeState(JumpChargeTime, () => GetChaseState()); 
        }
    }
}