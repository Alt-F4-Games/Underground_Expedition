using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private float lastAttackTime = -999f;

    public EnemyAttackState(EnemyAI enemy) : base(enemy) { }

    public override void Enter()
    {
        enemy.agent.isStopped = true;
    }

    public override void UpdateLogic()
    {
        if (enemy.player == null)
        {
            stateMachine.ChangeState(new EnemyPatrolState(enemy));
            return;
        }

        // mirar hacia el jugador
        enemy.transform.LookAt(new Vector3(enemy.player.position.x, enemy.transform.position.y, enemy.player.position.z));

        float dist = Vector3.Distance(enemy.transform.position, enemy.player.position);

        // si se aleja, volver a persecución
        if (dist > enemy.attackRange + 0.5f)
        {
            stateMachine.ChangeState(new EnemyChaseState(enemy));
            return;
        }

        if (Time.time >= lastAttackTime + enemy.attackCooldown)
        {
            DoAttack();
            lastAttackTime = Time.time;
        }
    }

    private void DoAttack()
    {
        Collider[] hits = Physics.OverlapSphere(
            enemy.transform.position + enemy.transform.forward * 0.5f,
            enemy.attackRange,
            enemy.playerMask
        );

        foreach (var c in hits)
        {
            // Buscamos cualquier objeto con HealthSystem o IDamageable
            Debug.Log("Collider detectado por ataque: " + c.name);
            if (c.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(enemy.attackDamage);
                Debug.Log($"{enemy.name} hizo {enemy.attackDamage} de daño a {c.name}");
            }
        }
    }
}