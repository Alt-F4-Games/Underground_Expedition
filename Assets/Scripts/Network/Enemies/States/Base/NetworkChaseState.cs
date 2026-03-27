using UnityEngine;

namespace Network.Enemies
{
    // State for pursuing the detected target
    public class NetworkChaseState : INetworkState
    {
        private NetworkEnemyController _enemy;

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = enemy;
            _enemy.Agent.isStopped = false; // Allow movement
            Debug.Log($"[SERVER] {_enemy.gameObject.name} started CHASING the player.");
        }

        public void Update()
        {
            // TRANSITION: Target lost or out of sight -> return to patrol
            if (_enemy.TargetPlayer == null)
            {
                _enemy.StateMachine.ChangeState(_enemy.GetPatrolState());
                return;
            }

            float distanceToTarget = Vector3.Distance(_enemy.transform.position, _enemy.TargetPlayer.transform.position);

            // TRANSITION: Target reached -> start attacking
            if (distanceToTarget <= _enemy.AttackRange)
            {
                _enemy.StateMachine.ChangeState(_enemy.GetAttackState());
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