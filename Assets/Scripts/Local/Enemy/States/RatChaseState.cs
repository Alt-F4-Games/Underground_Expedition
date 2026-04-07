using UnityEngine;
using Enemy.States.Base;

namespace Enemy.States.Rat
{
    public class RatChaseState : EnemyChaseState
    {
        private RatIA rat;

        public RatChaseState(RatIA enemy) : base(enemy)
        {
            rat = enemy;
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            if (rat.player == null)
            {
                stateMachine.ChangeState(new RatPatrolState(rat));
                return;
            }

            float dist = Vector3.Distance(rat.transform.position, rat.player.position);
            
            if (rat.CanJumpAttack())
            {
                stateMachine.ChangeState(new RatJumpState(rat));
                return;
            }
            
            if (!rat.CanSeePlayer())
            {
                stateMachine.ChangeState(new RatPatrolState(rat));
            }
        }
    }
}

