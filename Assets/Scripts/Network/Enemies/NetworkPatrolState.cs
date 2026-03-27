using UnityEngine;

namespace Network.Enemies
{
    public class NetworkPatrolState : INetworkState
    {
        private NetworkEnemyController _enemy;
        private int _currentIndex = 0;

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = enemy;
            
            if (_enemy.PatrolPath == null || _enemy.PatrolPath.Waypoints.Count == 0)
            {
                Debug.LogWarning($"[SERVER] {_enemy.gameObject.name} does not have a NetworkPatrolPath assigned.");
                return;
            }

            // Enable movement and move to the first waypoint
            _enemy.Agent.isStopped = false;
            MoveToNextWaypoint();
            
            Debug.Log($"[SERVER] {_enemy.gameObject.name} started patrolling.");
        }

        public void Update()
        {
            if (_enemy.PatrolPath == null || _enemy.PatrolPath.Waypoints.Count == 0) return;

            // If we reached the destination, move to the next one
            if (!_enemy.Agent.pathPending && _enemy.Agent.remainingDistance <= _enemy.PatrolPath.WaypointTolerance)
            {
                _currentIndex = (_currentIndex + 1) % _enemy.PatrolPath.Waypoints.Count;
                MoveToNextWaypoint();
            }
        }

        private void MoveToNextWaypoint()
        {
            Transform target = _enemy.PatrolPath.GetWaypoint(_currentIndex);
            if (target != null)
            {
                _enemy.Agent.SetDestination(target.position);
            }
        }

        public void Exit() 
        {
            if (_enemy.Agent != null && _enemy.Agent.isOnNavMesh)
            {
                _enemy.Agent.isStopped = true;
            }
        }

        public NetworkEnemyState GetStateType() => NetworkEnemyState.Patrolling;
    }
}