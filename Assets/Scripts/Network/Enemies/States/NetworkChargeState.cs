using Fusion; // Added for Runner.DeltaTime
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
            // Added !IsValid safety check for Fusion network objects
            if (_enemy.TargetPlayer == null || !_enemy.TargetPlayer.IsValid)
            {
                _enemy.StateMachine.ChangeState(_enemy.GetPatrolState());
                return;
            }
            
            Vector3 directionToPlayer = _enemy.TargetPlayer.transform.position - _enemy.transform.position;
            directionToPlayer.y = 0; // Ignore the Y axis so the enemy doesn't tilt up/down
            
            if (directionToPlayer.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                // Slerp for smooth rotation using Fusion's DeltaTime
                _enemy.transform.rotation = Quaternion.Slerp(_enemy.transform.rotation, targetRotation, _enemy.Runner.DeltaTime * 10f);
            }

            float distance = Vector3.Distance(_enemy.transform.position, _enemy.TargetPlayer.transform.position);

            // TRANSITION: Player moves out of range -> cancel charge and resume chase
            if (distance > _enemy.AttackRange)
            {
                _enemy.StateMachine.ChangeState(_enemy.GetChaseState());
                return;
            }

            // TIMING LOGIC: Advance the charge timer
            _timer += _enemy.Runner.DeltaTime;
            
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