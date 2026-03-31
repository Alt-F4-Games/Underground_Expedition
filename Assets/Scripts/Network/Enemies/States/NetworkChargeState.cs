using UnityEngine;
using System;

namespace Network.Enemies.States
{
    /// <summary>
    /// State for telegraphing or charging an ability before execution.
    /// Locks the enemy in place and transitions to a follow-up state upon completion.
    /// </summary>
    public class NetworkChargeState : INetworkState
    {
        private NetworkEnemyController _enemy;
        private float _chargeTime;
        private float _timer;
        private Func<INetworkState> _getNextState;
        
        public NetworkChargeState(float chargeTime, Func<INetworkState> getNextState)
        {
            _chargeTime = chargeTime;
            _getNextState = getNextState;
        }

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = enemy;
            
            // Lock the enemy in place to telegraph the upcoming attack
            _enemy.Agent.isStopped = true; 
            _timer = 0f;
            
            // TODO: Trigger "Charge" animation in the Animator
            Debug.Log($"[SERVER] {_enemy.gameObject.name} entered Charge/Telegraph state...");
        }

        public void Update()
        {
            // TRANSITION: Target lost or killed -> return to patrol
            if (_enemy.TargetPlayer == null)
            {
                _enemy.StateMachine.ChangeState(_enemy.GetPatrolState());
                return;
            }

            float distance = Vector3.Distance(_enemy.transform.position, _enemy.TargetPlayer.transform.position);

            // TRANSITION: Player moves out of range -> cancel charge and resume chase
            if (distance > _enemy.AttackRange)
            {
                _enemy.StateMachine.ChangeState(_enemy.GetChaseState());
                return;
            }

            // TIMING LOGIC: Advance the charge timer
            _timer += Time.deltaTime;
            
            if (_timer >= _chargeTime)
            {
                Debug.Log($"[SERVER] {_enemy.gameObject.name} finished charging. Transitioning to next state.");
                
                // Execute the callback to transition to the actual attack (e.g., JumpAttack or BasicAttack)
                if (_getNextState != null)
                {
                    _enemy.StateMachine.ChangeState(_getNextState.Invoke());
                }
            }
        }

        public void Exit() 
        {
            // Logic when exiting the charge state (empty for now)
        }

        public NetworkEnemyState GetStateType() => NetworkEnemyState.Charging;
    }
}