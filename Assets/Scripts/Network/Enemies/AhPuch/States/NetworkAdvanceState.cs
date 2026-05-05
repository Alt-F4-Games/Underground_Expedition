using Network.Enemies.Variants;
using UnityEngine;
using Network.Enemies;

namespace Network.Enemies.States
{
    public class NetworkAdvanceState : INetworkState
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
            
            Debug.Log($"[SERVER] Boss started advancing through its path.");
        }

        public void Update()
        {
            if (_enemy.PatrolPath == null || _enemy.PatrolPath.Waypoints.Count == 0) return;
            if (!_enemy.Agent.isOnNavMesh) return;
            
            if (_enemy.LookForTarget())
            {
                // Transition via factory method instead of hardcoding
                _enemy.StateMachine.ChangeState(_enemy.GetChaseState());
                return;
            }

            // Check if the Agent reached its current destination
            if (!_enemy.Agent.pathPending && _enemy.Agent.remainingDistance <= _enemy.PatrolPath.WaypointTolerance)
            {
                Transform currentWaypoint = _enemy.PatrolPath.GetWaypoint(_enemy.CurrentPathIndex);
                if (currentWaypoint != null)
                {
                    var statNode = currentWaypoint.GetComponent<AhPuchStatNode>();
                    if (statNode != null)
                    {
                        _enemy.ApplyStatNode(statNode);
                    }
                    
                    var evalNode = currentWaypoint.GetComponent<AhPuchEvalNode>();
                    if (evalNode != null)
                    {
                        if (_enemy.CurrentPathIndex < _enemy.PatrolPath.Waypoints.Count - 1)
                        {
                            _enemy.CurrentPathIndex++;
                        }
                        
                        _enemy.Agent.isStopped = true;
                        _enemy.EvaluateAndDecide();
                        
                        return; 
                    }
                }
                
                if (_enemy.CurrentPathIndex < _enemy.PatrolPath.Waypoints.Count - 1)
                {
                    _enemy.CurrentPathIndex++;
                    MoveToNextWaypoint();
                }
                else
                {
                    Debug.Log("[SERVER] Boss reached the end of the temple.");
                    _enemy.Agent.isStopped = true;
                }
            }
        }

        private void MoveToNextWaypoint()
        {
            Transform target = _enemy.PatrolPath.GetWaypoint(_enemy.CurrentPathIndex);
            
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