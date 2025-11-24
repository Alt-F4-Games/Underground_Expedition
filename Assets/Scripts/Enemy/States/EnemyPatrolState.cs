/*
    EnemyPatrolState.cs
    Base patrol state used by all enemy types.
    Makes the enemy move between waypoints in a loop.

    Dependencies:
    - EnemyState (abstract base class)
    - EnemyAI (movement controller with NavMeshAgent)
    - PatrolPath (provides waypoint positions)
*/

using UnityEngine;

namespace Enemy.States.Base
{
    public class EnemyPatrolState : EnemyState
    {
        // Keeps track of the current waypoint index in the patrol path
        protected int currentIndex = 0;

        public EnemyPatrolState(EnemyAI enemy) : base(enemy) { }

        public override void Enter()
        {
            // If no patrol path exists, stop all movement
            if (enemy.patrolPath == null || enemy.patrolPath.Count == 0)
            {
                enemy.agent.isStopped = true;
                return;
            }

            // Enable movement and set the first destination
            enemy.agent.isStopped = false;
            enemy.agent.SetDestination(enemy.patrolPath.GetWaypoint(currentIndex).position);
        }

        public override void UpdateLogic()
        {
            // Wait until the enemy reaches its current waypoint
            if (!enemy.agent.pathPending && enemy.agent.remainingDistance <= enemy.waypointTolerance)
            {
                // Move to next waypoint (looping back to start)
                currentIndex = (currentIndex + 1) % enemy.patrolPath.Count;

                // Update the agent's destination
                enemy.agent.SetDestination(enemy.patrolPath.GetWaypoint(currentIndex).position);
            }
        }
    }
}