using UnityEngine;

namespace Enemy.States.Base
{
    public class EnemyChaseState : EnemyState
    {
        public EnemyChaseState(EnemyAI enemy) : base(enemy) { }

        public override void Enter()
        {
            enemy.agent.isStopped = false;
        }

        public override void UpdateLogic()
        {
            if (enemy.player == null)
                return;

            enemy.agent.SetDestination(enemy.player.position);
        }
    }
}