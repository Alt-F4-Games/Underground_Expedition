using UnityEngine;
using Enemy.States.Base;

namespace Enemy.States.Skull
{
    public class SkullChaseState : EnemyChaseState
    {
        private SkullIA skull;
        private float lastAttackTime = -999f;

        public SkullChaseState(SkullIA enemy) : base(enemy)
        {
            skull = enemy;
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            
            if (!skull.CanSeePlayer())
            {
                stateMachine.ChangeState(new SkullPatrolState(skull));
                return;
            }
            
            if (skull.DistanceToPlayer() <= skull.attackRange)
            {
                if (Time.time >= lastAttackTime + skull.attackCooldown)
                {
                    skull.TryAttack();
                    lastAttackTime = Time.time;
                }
            }
        }
    }
}