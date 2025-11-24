/*
 * EnemyHealth
 * Concrete HealthSystem implementation for enemies.
 * Stops navigation and disables the AI on death.
 * Optionally destroys the GameObject after a delay.
 *
 * Dependencies:
 * - Requires NavMeshAgent
 * - Requires EnemyAI
 * - Inherits from HealthSystem (external)
 */

using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public class EnemyHealth : HealthSystem
    {
        private NavMeshAgent _agent;   // Movement component
        private EnemyAI _enemyAI;      // Behavior controller

        private void Start()
        {
            // Cache dependencies
            _agent = GetComponent<NavMeshAgent>();
            _enemyAI = GetComponent<EnemyAI>();
        }

        public override void Death()
        {
            // Call base death behavior
            base.Death();

            // Stop movement
            if (_agent != null) _agent.isStopped = true;

            // Disable the AI logic
            if (_enemyAI != null)
            {
                _enemyAI.enabled = false;
            }

            Debug.Log($"{gameObject.name} ha muerto. Desactivando IA...");

            // Destroy enemy after a delay (optional)
            Destroy(gameObject, 2f);
        }
    }
}