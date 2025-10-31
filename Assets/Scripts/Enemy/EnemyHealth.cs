using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
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
            
            Debug.Log($"{gameObject.name} ha muerto. Desactivando IA...");
            Destroy(gameObject, 2f); // opcional
        }
    }
}