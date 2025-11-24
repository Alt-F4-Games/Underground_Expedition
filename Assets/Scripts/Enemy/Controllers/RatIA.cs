/*
    RatIA.cs
    Description:
    Specialized AI for the Rat enemy, extending the base EnemyAI behavior.
    Adds jump-attack capabilities, initializes its own state machine with rat-specific states,
    and defines jump distance logic and debug gizmos.

    Dependencies:
    - EnemyAI (base class providing movement, detection, and general AI behavior)
    - EnemyStateMachine (handles state transitions)
    - Rat-specific states located under Enemy.States.Rat (e.g., RatPatrolState)
    - UnityEngine for transforms, drawing gizmos, etc.
*/

using UnityEngine;
using Enemy.States.Rat;
using Enemy.States.Base;

namespace Enemy
{
    public class RatIA : EnemyAI
    {
        [Header("Rat Jump Settings")]
        public float jumpExtraDistance = 2f;   // Extra distance added to jump trajectory
        public float jumpHeight = 3f;          // Height reached during the jump
        public float jumpSpeed = 8f;           // Speed at which the jump is performed
        public float jumpChargeTime = 1f;      // Time spent "charging" before jumping

        [Header("Rat Jump Range")]
        public float minJumpDistance = 3f;     // Minimum distance required to trigger a jump attack
        public float maxJumpDistance = 8f;     // Maximum distance allowed to trigger a jump attack

        protected override void Start()
        {
            base.Start(); // Calls shared initialization from EnemyAI
            stateMachine.Initialize(new RatPatrolState(this)); // Starts in the rat patrol state
        }

        // Determines whether the rat is allowed to perform a jump attack
        public bool CanJumpAttack()
        {
            if (player == null)
                return false;

            float dist = DistanceToPlayer(); // Get player distance from base class

            // Can jump only if target is in range and visible
            return dist >= minJumpDistance && 
                   dist <= maxJumpDistance && 
                   CanSeePlayer();
        }

        // Draws gizmos to visualize jump ranges in the editor
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Gizmos.color = Color.green; // Minimum jump radius
            Gizmos.DrawWireSphere(transform.position, minJumpDistance);

            Gizmos.color = new Color(1f, 0.6f, 0f, 1f); // Maximum jump radius (orange)
            Gizmos.DrawWireSphere(transform.position, maxJumpDistance);
        }
    }
}
