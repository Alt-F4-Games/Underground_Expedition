/*
    SkullChaseState.cs
    Chase state for Skull enemies.
    Extends EnemyChaseState and adds:
    - Lose player → return to patrol
    - Attack player when in range and cooldown is ready

    Dependencies:
    - EnemyChaseState (basic chase logic)
    - SkullIA (skull-specific settings)
    - SkullPatrolState (fallback behavior)
*/

using UnityEngine;
using Enemy.States.Base;

namespace Enemy.States.Skull
{
    public class SkullChaseState : EnemyChaseState
    {
        private SkullIA skull;             // Local typed reference
        private float lastAttackTime = -999f; // Tracks attack cooldown timing

        public SkullChaseState(SkullIA enemy) : base(enemy)
        {
            skull = enemy;
        }

        public override void UpdateLogic()
        {
            // Perform the default chase movement
            base.UpdateLogic();
            
            // If the skull cannot see the player anymore, return to patrol
            if (!skull.CanSeePlayer())
            {
                stateMachine.ChangeState(new SkullPatrolState(skull));
                return;
            }

            // If in attack range, check cooldown & attack
            if (skull.DistanceToPlayer() <= skull.attackRange)
            {
                if (Time.time >= lastAttackTime + skull.attackCooldown)
                {
                    skull.TryAttack();       // Execute an attack
                    lastAttackTime = Time.time; // Reset cooldown timer
                }
            }
        }
    }
}