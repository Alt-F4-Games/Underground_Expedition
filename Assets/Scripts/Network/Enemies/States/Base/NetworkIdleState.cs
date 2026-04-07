using UnityEngine;

namespace Network.Enemies
{
    // Default state where the enemy does nothing (useful for testing)
    public class NetworkIdleState : INetworkState
    {
        private NetworkEnemyController _enemy;

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = enemy;
            Debug.Log($"[SERVER] {_enemy.gameObject.name} entered IDLE state.");
        }

        public void Update() 
        { 
            // Waiting logic goes here
        }

        public void Exit() 
        {
            Debug.Log($"[SERVER] {_enemy.gameObject.name} exited IDLE state.");
        }

        public NetworkEnemyState GetStateType() => NetworkEnemyState.Idle;
    }
}