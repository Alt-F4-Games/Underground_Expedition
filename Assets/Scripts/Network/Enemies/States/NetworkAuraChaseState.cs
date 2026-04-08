using UnityEngine;
using Network.Enemies;

namespace Network.Enemies.States
{
    public class NetworkAuraChaseState : INetworkState
    {
        private NetworkEnemyController _enemy;

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = enemy;
            _enemy.Agent.isStopped = false; // Allow movement
            Debug.Log($"[SERVER] {_enemy.gameObject.name} started AURA CHASING the player.");
        }

        public void Update()
        {
            // Target lost or out of sight -> return to patrol
            if (_enemy.TargetPlayer == null)
            {
                _enemy.StateMachine.ChangeState(_enemy.GetPatrolState());
                return;
            }
            

            // Keep updating the destination to the player's current position
            _enemy.Agent.isStopped = false;
            _enemy.Agent.SetDestination(_enemy.TargetPlayer.transform.position);
        }

        public void Exit()
        {
            // Stop the agent when leaving the chase state
            if (_enemy.Agent != null && _enemy.Agent.isOnNavMesh)
            {
                _enemy.Agent.isStopped = true;
            }
        }

        public NetworkEnemyState GetStateType() => NetworkEnemyState.Chasing;
    }
}