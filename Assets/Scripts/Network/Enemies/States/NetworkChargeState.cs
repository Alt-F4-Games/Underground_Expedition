using UnityEngine;
using System;

namespace Network.Enemies.States
{
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
            _enemy.Agent.isStopped = true; // Lock the enemy in place
            _timer = 0f;
            
            // TODO: Add generic Animator Trigger here (e.g., enemy.Animator.SetTrigger("Charge"))
            Debug.Log($"[SERVER] {_enemy.gameObject.name} entered Charge/Telegraph state...");
        }

        public void Update()
        {
            if (_enemy.TargetPlayer == null)
            {
                _enemy.StateMachine.ChangeState(_enemy.GetPatrolState());
                return;
            }

            float distance = Vector3.Distance(_enemy.transform.position, _enemy.TargetPlayer.transform.position);

            // If the player moves out of attack range, cancel the charge
            if (distance > _enemy.AttackRange)
            {
                // TODO: Add Trigger to cancel the animation if necessary
                _enemy.StateMachine.ChangeState(_enemy.GetChaseState());
                return;
            }

            // Charge timer logic
            _timer += Time.deltaTime;
            
            if (_timer >= _chargeTime)
            {
                Debug.Log($"[SERVER] {_enemy.gameObject.name} finished charging. Transitioning to next state...");
                
                // Execute the function passed via constructor to transition (e.g., to a jump or attack)
                if (_getNextState != null)
                {
                    _enemy.StateMachine.ChangeState(_getNextState.Invoke());
                }
            }
        }

        public void Exit() 
        {
            
        }

        public NetworkEnemyState GetStateType() => NetworkEnemyState.Charging;
    }
}