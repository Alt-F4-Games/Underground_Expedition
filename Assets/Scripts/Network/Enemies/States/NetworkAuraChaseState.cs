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
                ReturnToPatrol();
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(_enemy.transform.position, _enemy.TargetPlayer.transform.position);
            
            if (distanceToPlayer > _enemy.VisionRange + 2f)
            {
                Debug.Log($"[SERVER] {_enemy.gameObject.name} lost sight of the player.");
                ReturnToPatrol();
                return;
            }

            // NODE RECOGNITION DURING CHASE
            if (_enemy is NetworkAhPuchController boss)
            {
                CheckForNearbyNodes(boss);
            }

            // Keep updating the destination to the player's current position
            _enemy.Agent.isStopped = false;
            _enemy.Agent.SetDestination(_enemy.TargetPlayer.transform.position);
        }

        private void CheckForNearbyNodes(NetworkAhPuchController boss)
        {
            if (boss.PatrolPath == null) return;

            // Iterate through nodes to see if we are passing over any stat nodes
            for (int i = 0; i < boss.PatrolPath.Waypoints.Count; i++)
            {
                Transform wp = boss.PatrolPath.GetWaypoint(i);
                if (wp == null) continue;
                
                if (Vector3.Distance(boss.transform.position, wp.position) <= boss.ChaseNodeDetectionRadius)
                {
                    // If the node has stats, apply them immediately (e.g., room change)
                    var statNode = wp.GetComponent<AhPuchStatNode>();
                    if (statNode != null)
                    {
                        boss.ApplyStatNode(statNode);
                    }
                    
                    // Synchronize index so it knows its current position on the map
                    boss.CurrentPathIndex = i;
                    break; // Prevent processing multiple nodes in the same frame
                }
            }
        }

        private void ReturnToPatrol()
        {
            if (_enemy is NetworkAhPuchController bossController)
            {
                bossController.SetNearestPathIndex();
            }

            _enemy.StateMachine.ChangeState(_enemy.GetPatrolState());
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