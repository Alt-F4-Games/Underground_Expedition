using Fusion;
using UnityEngine;
using System;

namespace Network.Enemies.States
{
    public class NetworkResettableChargeState : INetworkState
    {
        private NetworkEnemyController _enemy;

        private float _chargeTime;
        private float _timer;

        private Func<bool> _shouldReset;
        private Action _onReset;

        private Func<INetworkState> _getNextState;

        public NetworkResettableChargeState(
            float chargeTime,
            Func<bool> shouldReset,
            Action onReset,
            Func<INetworkState> getNextState)
        {
            _chargeTime = chargeTime;
            _shouldReset = shouldReset;
            _onReset = onReset;
            _getNextState = getNextState;
        }

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = enemy;

            if (_enemy.Agent != null)
                _enemy.Agent.isStopped = true;

            _timer = 0f;
        }

        public void Update()
        {
            if (_enemy == null) return;
            
            if (_shouldReset != null && _shouldReset.Invoke())
            {
                _timer = 0f;
                _onReset?.Invoke();
            }

            _timer += _enemy.Runner.DeltaTime;

            if (_timer >= _chargeTime)
            {
                _enemy.StateMachine.ChangeState(_getNextState.Invoke());
            }
        }

        public void Exit() { }

        public NetworkEnemyState GetStateType() => NetworkEnemyState.Charging;
    }
}