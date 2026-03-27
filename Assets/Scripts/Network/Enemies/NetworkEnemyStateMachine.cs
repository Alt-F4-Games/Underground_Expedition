namespace Network.Enemies
{
    public class NetworkEnemyStateMachine
    {
        private NetworkEnemyController _enemy;
        public INetworkState CurrentState { get; private set; }

        public NetworkEnemyStateMachine(NetworkEnemyController enemy)
        {
            _enemy = enemy;
        }

        public void ChangeState(INetworkState newState)
        {
            if (newState == null || newState == CurrentState) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter(_enemy);

            // Synchronize state through the controller
            _enemy.CurrentState = newState.GetStateType();
        }

        public void Update()
        {
            CurrentState?.Update();
        }
    }
}