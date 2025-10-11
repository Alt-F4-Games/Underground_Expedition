using UnityEngine;

public class EnemyPatrolState : EnemyState
{
    private int currentIndex = 0;

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
        if (enemy.CanSeePlayer())
        {
            Debug.Log("Jugador detectado — cambiando a persecución");
            stateMachine.ChangeState(new EnemyChaseState(enemy));
            return;
        }

        if (!enemy.agent.pathPending && enemy.agent.remainingDistance <= enemy.waypointTolerance)
        {
            currentIndex = (currentIndex + 1) % enemy.patrolPath.Count;
            enemy.agent.SetDestination(enemy.patrolPath.GetWaypoint(currentIndex).position);
        }

    }
}