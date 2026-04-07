using UnityEngine;
using Enemy.States.Base;

namespace Enemy.States.Rat
{
    public class RatPatrolState : EnemyPatrolState
    {
        private RatIA rat;

        public RatPatrolState(RatIA enemy) : base(enemy)
        {
            rat = enemy;
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            if (rat.CanSeePlayer())
            {
                if (rat.CanJumpAttack())
                {
                    stateMachine.ChangeState(new RatJumpState(rat));
                    return;
                }

                stateMachine.ChangeState(new RatChaseState(rat));
            }
        }
    }
}