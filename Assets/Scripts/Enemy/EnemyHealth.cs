using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : HealthSystem
{
    private NavMeshAgent _agent;
    private EnemyAI _enemyAI;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemyAI = GetComponent<EnemyAI>();
    }

    public override void Death()
    {
        base.Death();

        if (_agent != null) _agent.isStopped = true;
        if (_enemyAI != null)
        {
            _enemyAI.enabled = false;
        }

        // Podés reemplazar esto por animación de muerte o efecto
        Debug.Log($"{gameObject.name} ha muerto. Desactivando IA...");
        Destroy(gameObject, 2f); // opcional
    }
}