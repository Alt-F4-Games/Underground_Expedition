using UnityEngine;
using System.Collections.Generic;
using Network.Spawn;

namespace Network.Enemies.States
{
    public class AhPuchInvokeState : INetworkState
    {
        private NetworkAhPuchController _enemy;
        private List<SummonPoint> _zonesToTrigger;
        private float _invokeDelay = 1.5f;
        private float _timer;
        private bool _hasSummoned;

        public AhPuchInvokeState(List<SummonPoint> zones)
        {
            _zonesToTrigger = zones;
        }

        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = (NetworkAhPuchController)enemy;
            _timer = 0f;
            _hasSummoned = false;
            
            _enemy.Agent.isStopped = true;
            _enemy.Agent.velocity = Vector3.zero;

            Debug.Log("[SERVER] Ah Puch invocation started.");
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
                _enemy.StateMachine.ChangeState(new AhPuchDashState(_enemy.DashDurationSuccess));
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