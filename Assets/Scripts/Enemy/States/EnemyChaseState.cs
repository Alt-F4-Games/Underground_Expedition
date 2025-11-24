/*
    EnemyChaseState.cs
    Base chase state for all enemies.
    Makes the enemy follow the player's position continuously.

    Dependencies:
    - EnemyState (base abstract class)
    - EnemyAI (provides movement and destination logic)
*/

using UnityEngine;

namespace Enemy.States.Base
{
    public class EnemyChaseState : EnemyState
    {
        public EnemyChaseState(EnemyAI enemy) : base(enemy) { }

        public override void Enter()
        {
            // Allow the NavMeshAgent to move normally when the chase begins
            enemy.agent.isStopped = false;
        }

        public override void UpdateLogic()
        {
            // If the player does not exist, the state cannot function
            if (enemy.player == null)
                return;

            // Continuously move toward the player's last known position
            enemy.agent.SetDestination(enemy.player.position);
        }
    }
}