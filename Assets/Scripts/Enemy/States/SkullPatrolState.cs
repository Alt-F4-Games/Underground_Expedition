/*
    SkullPatrolState.cs
    Patrol state for the Skull enemy.
    Uses standard patrol logic and transitions into chase when the player is detected.

    Dependencies:
    - EnemyPatrolState (base waypoint movement)
    - SkullIA (enemy-specific vision logic)
*/

using UnityEngine;
using Enemy.States.Base;

namespace Enemy.States.Skull
{
    public class SkullPatrolState : EnemyPatrolState
    {
        // Typed reference for SkullIA-specific methods
        private SkullIA skull;

        public SkullPatrolState(SkullIA enemy) : base(enemy)
        {
            skull = enemy;
        }

        public override void UpdateLogic()
        {
            // Execute base patrol behavior
            base.UpdateLogic();

            // If the skull detects the player, transition to chase state
            if (skull.CanSeePlayer())
                stateMachine.ChangeState(new SkullChaseState(skull));
        }
    }
}