using UnityEngine;

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
        {
            stateMachine.ChangeState(new EnemyPatrolState(enemy));
            return;
        }

        enemy.agent.SetDestination(enemy.player.position);

        float dist = Vector3.Distance(enemy.transform.position, enemy.player.position);

        if (dist <= enemy.attackRange)
        {
            stateMachine.ChangeState(new EnemyAttackState(enemy));
        }
        else if (!enemy.CanSeePlayer())
        {
            stateMachine.ChangeState(new EnemyPatrolState(enemy));
        }
    }
}