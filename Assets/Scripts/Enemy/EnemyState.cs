using UnityEngine;

public abstract class EnemyState
{
    protected EnemyAI enemy;
    protected EnemyStateMachine stateMachine;

    protected EnemyState(EnemyAI enemy)
    {
        this.enemy = enemy;
        this.stateMachine = enemy.stateMachine;
    }

    public virtual void Enter() { }
    public virtual void UpdateLogic() { }
    public virtual void UpdatePhysics() { }
    public virtual void Exit() { }
}