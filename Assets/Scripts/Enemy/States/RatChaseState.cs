/*
    RatChaseState.cs
    Chase state specifically for Rat enemies.
    Extends the base chase logic and adds:
    - Transition to jump attack when possible
    - Transition back to patrol if the player is lost

    Dependencies:
    - EnemyChaseState (base chase behavior)
    - RatIA (rat-specific AI data)
    - RatPatrolState, RatJumpState (state transitions)
*/

using UnityEngine;
using Enemy.States.Base;

namespace Enemy.States.Rat
{
    public class RatChaseState : EnemyChaseState
    {
        private RatIA rat; // Local reference for rat-specific data

        public RatChaseState(RatIA enemy) : base(enemy)
        {
            rat = enemy;
        }

        public override void UpdateLogic()
        {
            // Run the generic chase logic from the parent state
            base.UpdateLogic();

            // If the player no longer exists, return to patrol
            if (rat.player == null)
            {
                stateMachine.ChangeState(new RatPatrolState(rat));
                return;
            }

            // Distance to player for additional logic checks
            float dist = Vector3.Distance(rat.transform.position, rat.player.position);

            // If rat can perform a jump attack, switch to that state
            if (rat.CanJumpAttack())
            {
                stateMachine.ChangeState(new RatJumpState(rat));
                return;
            }

            // If rat loses visual contact with player, go back to patrol
            if (!rat.CanSeePlayer())
            {
                stateMachine.ChangeState(new RatPatrolState(rat));
            }
        }
    }
}