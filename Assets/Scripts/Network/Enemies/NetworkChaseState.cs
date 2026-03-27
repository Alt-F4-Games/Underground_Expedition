using UnityEngine;

namespace Network.Enemies
{
    public class NetworkChaseState : INetworkState
    {
        private NetworkEnemyController _enemy;

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = enemy;
            _enemy.Agent.isStopped = false;
            Debug.Log($"[SERVER] {_enemy.gameObject.name} started CHASING the player.");
        }

        public void Update()
        {
            // If we lose sight of the player, return to patrolling
            if (_enemy.TargetPlayer == null)
            {
                _enemy.StateMachine.ChangeState(new NetworkPatrolState());
                return;
            }

            float distanceToTarget = Vector3.Distance(_enemy.transform.position, _enemy.TargetPlayer.transform.position);

            // If we reach attack range
            if (distanceToTarget <= _enemy.AttackRange)
            {
                _enemy.StateMachine.ChangeState(new NetworkAttackState());
                return;
            }

            // Follow the player
            _enemy.Agent.isStopped = false;
            _enemy.Agent.SetDestination(_enemy.TargetPlayer.transform.position);
        }

        public void Exit()
        {
            if (_enemy.Agent != null && _enemy.Agent.isOnNavMesh)
            {
                _enemy.Agent.isStopped = true;
            }
        }

        public NetworkEnemyState GetStateType() => NetworkEnemyState.Chasing;
    }
}