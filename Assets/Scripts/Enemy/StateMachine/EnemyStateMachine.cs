/*
 * EnemyStateMachine
 * This class handles switching between different enemy states.
 * It keeps track of the current state and provides safe transitions
 * by calling Enter() and Exit() on each state.
 *
 * Dependencies:
 * - Requires EnemyState-derived classes that implement Enter() and Exit().
 */

namespace Enemy
{
    public class EnemyStateMachine
    {
        // Holds the current active state of the enemy
        public EnemyState CurrentState { get; private set; }

        // Exposes the name of the current state for debugging or UI purposes
        public string CurrentStateName => CurrentState?.GetType().Name ?? "None";

        public void Initialize(EnemyState startState)
        {
            // Sets the initial state and calls its Enter() logic
            CurrentState = startState;
            startState.Enter();
        }

        public void ChangeState(EnemyState newState)
        {
            // Prevents changing to null or the same state
            if (newState == null || newState == CurrentState)
                return;

            // Exit current state before switching
            CurrentState?.Exit();

            // Switch to the new state and run its Enter() logic
            CurrentState = newState;
            newState.Enter();
        }
    }
}