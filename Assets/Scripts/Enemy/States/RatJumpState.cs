using UnityEngine;
using Enemy.States.Base;

namespace Enemy.States.Rat
{
    /*
        RatJumpState.cs
        Jump attack logic specifically for rat enemies.
        Extends the base EnemyJumpState to add:

        - Rat-specific jump parameters
        - Mid-air attack only once per jump
        - State transitions after landing
        - Cooldown handling

        This is the rat’s strongest attack behavior.
    */

    public class RatJumpState : EnemyJumpState
    {
        private RatIA rat;
        private float lastAttackTime = -999f;  // Timer for attack cooldown
        private bool hasAttackedDuringThisJump = false; // Ensures 1 attack per jump

        public RatJumpState(RatIA enemy) : base(enemy)
        {
            rat = enemy;

            // Inherit settings directly from RatIA inspector values
            extraDistance = rat.jumpExtraDistance;
            jumpHeight = rat.jumpHeight;
            jumpSpeed = rat.jumpSpeed;
            chargeTime = rat.jumpChargeTime;
        }

        public override void UpdateLogic()
        {
            // Run base jump logic (charging, landing detection)
            base.UpdateLogic();

            // No attack conditions if not fully in the jumping phase
            if (!isJumping || rat.player == null)
                return;

            float dist = rat.DistanceToPlayer();

            // Attack ONCE during the jump when close to the player
            if (!hasAttackedDuringThisJump 
                && dist <= rat.attackRange
                && Time.time >= lastAttackTime + rat.attackCooldown)
            {
                rat.TryAttack();
                lastAttackTime = Time.time;
                hasAttackedDuringThisJump = true;

                Debug.Log($"{rat.name} attacked {rat.player.name} once at {Time.time:F2}");
            }
        }

        protected override void StartJump()
        {
            // Reset attack permission before a new jump
            base.StartJump();
            hasAttackedDuringThisJump = false;
        }

        protected override void Land()
        {
            // Restore navmesh and stop jump
            base.Land();
            hasAttackedDuringThisJump = false;

            // State transitions after landing
            if (rat.player == null)
            {
                stateMachine.ChangeState(new RatPatrolState(rat));
                return;
            }

            // If still in sight
            if (rat.CanSeePlayer())
            {
                // If another jump is possible, chain jumps
                if (rat.CanJumpAttack())
                {
                    stateMachine.ChangeState(new RatJumpState(rat));
                    return;
                }

                // Otherwise resume chasing
                stateMachine.ChangeState(new RatChaseState(rat));
                return;
            }

            // Lost sight → return to patrol
            stateMachine.ChangeState(new RatPatrolState(rat));
        }
    }
}
