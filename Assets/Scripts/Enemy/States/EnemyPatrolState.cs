using UnityEngine;

namespace Enemy.States.Base
{
    public class EnemyPatrolState : EnemyState
    {
        protected int currentIndex = 0;

        public EnemyPatrolState(EnemyAI enemy) : base(enemy) { }

        public override void Enter()
        {
            if (enemy.patrolPath == null || enemy.patrolPath.Count == 0)
            {
                enemy.agent.isStopped = true;
                return;
            }

            enemy.agent.isStopped = false;
            enemy.agent.SetDestination(enemy.patrolPath.GetWaypoint(currentIndex).position);
        }

        public override void UpdateLogic()
        {
            if (!enemy.agent.pathPending && enemy.agent.remainingDistance <= enemy.waypointTolerance)
            {
                currentIndex = (currentIndex + 1) % enemy.patrolPath.Count;
                enemy.agent.SetDestination(enemy.patrolPath.GetWaypoint(currentIndex).position);
            }
        }
    }
}