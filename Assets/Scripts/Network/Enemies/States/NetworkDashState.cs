using Fusion;
using UnityEngine;
using Network.Enemies;
using Network.Enemies.Variants;

namespace Network.Enemies.States
{
    public class NetworkDashState : INetworkState
    {
        private NetworkAhPuchController _enemy;
        private float _dashDuration;
        private float _timer;

        // Constructor receiving how long the dash will last
        public NetworkDashState(float duration)
        {
            _dashDuration = duration;
        }

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = (NetworkAhPuchController)enemy;
            _timer = 0f;
            
            // Activate the dash flag and recalculate speed to apply the boost
            _enemy.IsDashing = true;
            _enemy.RecalculatePhaseStats();
            
            _enemy.Agent.isStopped = false;
            MoveToNextWaypoint();

            Debug.Log($"[SERVER] Boss started DASHING for {_dashDuration}s. Speed: {_enemy.Agent.speed}");
        }

        public void Update()
        {
            if (_enemy.PatrolPath == null || _enemy.PatrolPath.Waypoints.Count == 0) return;
            if (!_enemy.Agent.isOnNavMesh) return;

            // 1. Dash Timer
            _timer += _enemy.Runner.DeltaTime;
            if (_timer >= _dashDuration)
            {
                // Transition back to the patrol state via factory method
                _enemy.StateMachine.ChangeState(_enemy.GetPatrolState());
                return;
            }

            // Movement logic and node reading (Same as AdvanceState)
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
                }
                
                if (_enemy.CurrentPathIndex < _enemy.PatrolPath.Waypoints.Count - 1)
                {
                    _enemy.CurrentPathIndex++;
                    MoveToNextWaypoint();
                }
                else
                {
                    _enemy.Agent.isStopped = true;
                }
            }
            else
            {
                // Ensure it keeps heading to the current target
                MoveToNextWaypoint();
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
            // Turn off the flag and restore normal speed
            _enemy.IsDashing = false;
            _enemy.RecalculatePhaseStats();
            
            Debug.Log($"[SERVER] Boss finished DASHING. Speed back to: {_enemy.Agent.speed}");
        }

        // Return 'Chasing' so the aggressive run animation plays on clients
        public NetworkEnemyState GetStateType() => NetworkEnemyState.Chasing; 
    }
}