/*
 * EnemyState
 * Abstract base class for all enemy states.
 * Stores references to the EnemyAI and its state machine.
 * Provides lifecycle methods (Enter, UpdateLogic, UpdatePhysics, Exit)
 * that derived states can override to implement behavior.
 */

namespace Enemy
{
    public abstract class EnemyState
    {
        // Reference to the enemy this state belongs to
        protected EnemyAI enemy;

        // Cached reference to the state machine for transitions
        protected EnemyStateMachine stateMachine;

        protected EnemyState(EnemyAI enemy)
        {
            this.enemy = enemy;
            this.stateMachine = enemy.stateMachine;
        }

        // Called when the state becomes active
        public virtual void Enter() { }

        // Called every Update() while the state is active
        public virtual void UpdateLogic() { }

        // Called every FixedUpdate() for physics-related logic
        public virtual void UpdatePhysics() { }

        // Called before switching to another state
        public virtual void Exit() { }
    }
}