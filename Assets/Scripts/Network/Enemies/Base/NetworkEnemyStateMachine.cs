namespace Network.Enemies
{
    // Brain of the FSM. Runs only on the Host (StateAuthority)
    public class NetworkEnemyStateMachine
    {
        private NetworkEnemyController _enemy;
        public INetworkState CurrentState { get; private set; }

        public NetworkEnemyStateMachine(NetworkEnemyController enemy)
        {
            _enemy = enemy;
        }

        // Handles switching from one state to another cleanly
        public void ChangeState(INetworkState newState)
        {
            if (newState == null || newState == CurrentState) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter(_enemy);

            // Update the networked property to sync animations across all clients
            _enemy.CurrentState = newState.GetStateType();
        }

        // Executes the current state's logic
        public void Update()
        {
            CurrentState?.Update();
        }
    }
}