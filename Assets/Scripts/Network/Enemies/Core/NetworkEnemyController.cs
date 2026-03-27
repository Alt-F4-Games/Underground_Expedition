using Fusion;
using UnityEngine;
using UnityEngine.AI;

namespace Network.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NetworkEnemyController : NetworkBehaviour
    {
        [Header("References")]
        public NavMeshAgent Agent;
        public NetworkPatrolPath PatrolPath;

        [Header("Detection Settings")]
        public float VisionRange = 8f;
        public float AttackRange = 1.5f;
        public LayerMask PlayerLayer; // Layer used to find targets
        
        [Header("Attack Settings")]
        public int AttackDamage = 10;
        public float AttackCooldown = 1.2f;

        public NetworkEnemyStateMachine StateMachine { get; private set; }
        public NetworkObject TargetPlayer { get; private set; } // Current target being chased

        // Networked enum. Triggers OnStateChanged on all clients when updated by the Host
        [Networked, OnChangedRender(nameof(OnStateChanged))]
        public NetworkEnemyState CurrentState { get; set; }

        public override void Spawned()
        {
            Agent = GetComponent<NavMeshAgent>();

            // Initialize FSM only on the Host (Server)
            if (HasStateAuthority)
            {
                StateMachine = new NetworkEnemyStateMachine(this);
                StateMachine.ChangeState(new NetworkPatrolState());
            }
        }

        public override void FixedUpdateNetwork()
        {
            // Only the Host calculates AI logic
            if (HasStateAuthority)
            {
                FindTargetPlayer();
                StateMachine.Update();
            }
        }

        // Scans for the closest player within VisionRange
        private void FindTargetPlayer()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, VisionRange, PlayerLayer);
            float closestDistance = float.MaxValue;
            NetworkObject closestPlayer = null;

            foreach (var hit in hits)
            {
                var netObj = hit.GetComponentInParent<NetworkObject>();
                if (netObj != null)
                {
                    float distance = Vector3.Distance(transform.position, netObj.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlayer = netObj;
                    }
                }
            }

            TargetPlayer = closestPlayer;
        }

        // Triggered on all clients to update visuals/animations
        void OnStateChanged()
        {
            // TODO: Update Animator parameters based on CurrentState
        }

        // Draws debug spheres in the Unity Editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, VisionRange);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
        }
    }
}