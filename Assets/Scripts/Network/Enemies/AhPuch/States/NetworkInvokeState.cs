using UnityEngine;
using System.Collections.Generic;
using Network.Spawn;

namespace Network.Enemies.States
{
    public class NetworkInvokeState : INetworkState
    {
        private NetworkAhPuchController _enemy;
        private List<SummonPoint> _zonesToTrigger;
        private float _invokeDelay;
        private float _timer;
        private bool _hasSummoned;

        public NetworkInvokeState(List<SummonPoint> zones, float waitTime)
        {
            _zonesToTrigger = zones;
            _invokeDelay = waitTime;
        }

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = (NetworkAhPuchController)enemy;
            _timer = 0f;
            _hasSummoned = false;
            
            _enemy.Agent.isStopped = true;
            _enemy.Agent.velocity = Vector3.zero;

            Debug.Log($"[SERVER] Boss invocation started. Casting for {_invokeDelay}s.");
        }

        public void Update()
        {
            _timer += _enemy.Runner.DeltaTime;
            
            if (!_hasSummoned && _timer >= _invokeDelay)
            {
                _hasSummoned = true;
                ExecuteSummons();
            }
            
            if (_timer >= _invokeDelay + 0.5f)
            {
                // Transition to dash state via factory method
                _enemy.StateMachine.ChangeState(_enemy.GetDashState(_enemy.DashDurationSuccess, 0f));
            }
        }

        private void ExecuteSummons()
        {
            foreach (var zone in _zonesToTrigger)
            {
                if (zone != null) zone.TriggerSummon();
            }
        }

        public void Exit()
        {
        }
        
        public NetworkEnemyState GetStateType() => NetworkEnemyState.Attacking;
    }
}