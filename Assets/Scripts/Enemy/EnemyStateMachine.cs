namespace Enemy
{
    public class EnemyStateMachine
    {
        public EnemyState CurrentState { get; private set; }

        public void Initialize(EnemyState startState)
        {
            CurrentState = startState;
            startState.Enter();
        }

        public void ChangeState(EnemyState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            newState.Enter();
        }
    }
}