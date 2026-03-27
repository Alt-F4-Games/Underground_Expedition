namespace Network.Enemies
{
    public enum NetworkEnemyState 
    { 
        Idle, 
        Patrolling, 
        Chasing, 
        Attacking, 
        Dead 
    }

    public interface INetworkState
    {
        void Enter(NetworkEnemyController enemy);
        void Update();
        void Exit();
        NetworkEnemyState GetStateType();
    }
}