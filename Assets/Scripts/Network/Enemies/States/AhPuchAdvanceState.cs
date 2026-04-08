using UnityEngine;
using Network.Enemies;

namespace Network.Enemies.Variants
{
    public class AhPuchAdvanceState : INetworkState
    {
        private NetworkAhPuchController _enemy;

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = (NetworkAhPuchController)enemy;
            
            if (_enemy.PatrolPath == null || _enemy.PatrolPath.Waypoints.Count == 0)
            {
                Debug.LogWarning($"[SERVER] {_enemy.gameObject.name} does not have an AhPuchPath assigned.");
                return;
            }

            _enemy.Agent.isStopped = false;
            MoveToNextWaypoint();
            
            Debug.Log($"[SERVER] Ah Puch started advancing through its path.");
        }

        public void Update()
        {
            if (_enemy.PatrolPath == null || _enemy.PatrolPath.Waypoints.Count == 0) return;

            // If the agent is not yet snapped to the NavMesh, do nothing this frame.
            if (!_enemy.Agent.isOnNavMesh) return;

            // Check if the Agent reached its current destination
            if (!_enemy.Agent.pathPending && _enemy.Agent.remainingDistance <= _enemy.PatrolPath.WaypointTolerance)
            {
                // Check if the current waypoint is a stat modifier
                Transform currentWaypoint = _enemy.PatrolPath.GetWaypoint(_enemy.CurrentPathIndex);
                if (currentWaypoint != null)
                {
                    var statNode = currentWaypoint.GetComponent<AhPuchStatNode>();
                    if (statNode != null)
                    {
                        _enemy.ApplyStatNode(statNode);
                    }
                    
                    // TODO: Logic to check if current node is for Evaluation
                }
                
                if (_enemy.CurrentPathIndex < _enemy.PatrolPath.Waypoints.Count - 1)
                {
                    _enemy.CurrentPathIndex++;
                    MoveToNextWaypoint();
                }
                else
                {
                    Debug.Log("[SERVER] Ah Puch reached the end of the temple.");
                    _enemy.Agent.isStopped = true;
                }
            }
        }

        private void MoveToNextWaypoint()
        {
            Transform target = _enemy.PatrolPath.GetWaypoint(_enemy.CurrentPathIndex);
            
            // Check isOnNavMesh before setting destination
            if (target != null && _enemy.Agent.isOnNavMesh)
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