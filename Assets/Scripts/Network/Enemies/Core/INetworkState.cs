namespace Network.Enemies
{
    // Defines all possible states for the network synchronization
    public enum NetworkEnemyState 
    { 
        Idle, 
        Patrolling, 
        Chasing, 
        Attacking, 
        Dead,
        Charging,
        Jumping
        Attacking,
        Exploding,
        Dead 
    }

    // Base interface that all enemy states must implement
    public interface INetworkState
    {
        void Enter(NetworkEnemyController enemy); // Called once when entering the state
        void Update();                            // Called every tick by the State Machine
        void Exit();                              // Called once before leaving the state
        NetworkEnemyState GetStateType();         // Returns the enum for network syncing
    }
}