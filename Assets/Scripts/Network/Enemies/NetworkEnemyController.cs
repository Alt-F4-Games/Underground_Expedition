using Fusion;
using UnityEngine;

namespace Network.Enemies
{
    public class NetworkEnemyController : NetworkBehaviour
    {
        public NetworkEnemyStateMachine StateMachine { get; private set; }

        // Networked property. When the Host changes this, clients execute OnStateChanged
        [Networked, OnChangedRender(nameof(OnStateChanged))]
        public NetworkEnemyState CurrentState { get; set; }

        public override void Spawned()
        {
            // Only the Host creates and runs the AI
            if (HasStateAuthority)
            {
                StateMachine = new NetworkEnemyStateMachine(this);
                StateMachine.ChangeState(new NetworkIdleState());
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