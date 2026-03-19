using UnityEngine;
using Enemy.States.Base;

namespace Enemy.States.Skull
{
    public class SkullPatrolState : EnemyPatrolState
    {
        private SkullIA skull;

        public SkullPatrolState(SkullIA enemy) : base(enemy)
        {
            skull = enemy;
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            if (skull.CanSeePlayer())
                stateMachine.ChangeState(new SkullChaseState(skull));
        }
    }
}