namespace Enemy
{
    public class EnemyStateMachine
    {
        public EnemyState CurrentState { get; private set; }
        public string CurrentStateName => CurrentState?.GetType().Name ?? "None";

        public void Initialize(EnemyState startState)
        {
            CurrentState = startState;
            startState.Enter();
        }

        public void ChangeState(EnemyState newState)
        {
            if (newState == null || newState == CurrentState)
                return;

            CurrentState?.Exit();
            CurrentState = newState;
            newState.Enter();
        }
    }
}