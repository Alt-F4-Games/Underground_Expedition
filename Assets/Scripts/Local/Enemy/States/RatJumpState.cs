using UnityEngine;
using Enemy.States.Base;

namespace Enemy.States.Rat
{
    public class RatJumpState : EnemyJumpState
    {
        private RatIA rat;
        private float lastAttackTime = -999f;
        private bool hasAttackedDuringThisJump = false;

        public RatJumpState(RatIA enemy) : base(enemy)
        {
            rat = enemy;
            
            extraDistance = rat.jumpExtraDistance;
            jumpHeight = rat.jumpHeight;
            jumpSpeed = rat.jumpSpeed;
            chargeTime = rat.jumpChargeTime;
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            if (!isJumping || rat.player == null)
                return;

            float dist = rat.DistanceToPlayer();
            
            if (!hasAttackedDuringThisJump && dist <= rat.attackRange && Time.time >= lastAttackTime + rat.attackCooldown)
            {
                rat.TryAttack();
                lastAttackTime = Time.time;
                hasAttackedDuringThisJump = true;
                Debug.Log($"{rat.name} attacked {rat.player.name} once at {Time.time:F2}");
            }
        }

        protected override void StartJump()
        {
            base.StartJump();
            hasAttackedDuringThisJump = false;
        }

        protected override void Land()
        {
            base.Land();
            hasAttackedDuringThisJump = false;
            
            if (rat.player == null)
            {
                stateMachine.ChangeState(new RatPatrolState(rat));
                return;
            }

            if (rat.CanSeePlayer())
            {
                if (rat.CanJumpAttack())
                {
                    stateMachine.ChangeState(new RatJumpState(rat));
                    return;
                }

                stateMachine.ChangeState(new RatChaseState(rat));
                return;
            }

            stateMachine.ChangeState(new RatPatrolState(rat));
        }
    }
}
