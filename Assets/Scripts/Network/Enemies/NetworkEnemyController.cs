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

        public NetworkEnemyStateMachine StateMachine { get; private set; }

        // Synchronized property. When the Host changes this, clients execute OnStateChanged
        [Networked, OnChangedRender(nameof(OnStateChanged))]
        public NetworkEnemyState CurrentState { get; set; }

        public override void Spawned()
        {
            Agent = GetComponent<NavMeshAgent>();

            // Only the Host creates and runs the AI
            if (HasStateAuthority)
            {
                StateMachine = new NetworkEnemyStateMachine(this);
                StateMachine.ChangeState(new NetworkPatrolState());
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
            {
                StateMachine.Update();
            }
        }

        void OnStateChanged()
        {
            Debug.Log($"[NETWORK] Sync: {gameObject.name}'s state is now {CurrentState}");
        }
    }
}