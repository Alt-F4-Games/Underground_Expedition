/*
    RatPatrolState.cs
    Patrol state for the Rat enemy.
    Behaves like the base patrol but adds detection and jump-attack transitions.

    Dependencies:
    - EnemyPatrolState (base movement logic)
    - RatIA (enemy-specific vision and attack checks)
*/

using UnityEngine;
using Enemy.States.Base;

namespace Enemy.States.Rat
{
    public class RatPatrolState : EnemyPatrolState
    {
        // Strongly typed reference to the RatAI for specific behavior checks
        private RatIA rat;

        public RatPatrolState(RatIA enemy) : base(enemy)
        {
            rat = enemy;
        }

        public override void UpdateLogic()
        {
            // Perform the regular patrol logic
            base.UpdateLogic();

            // If the rat sees the player, decide the next behavior
            if (rat.CanSeePlayer())
            {
                // If a jump attack is possible, transition to jump state
                if (rat.CanJumpAttack())
                {
                    stateMachine.ChangeState(new RatJumpState(rat));
                    return;
                }

                // Otherwise, begin chasing the player
                stateMachine.ChangeState(new RatChaseState(rat));
            }
        }
    }
}