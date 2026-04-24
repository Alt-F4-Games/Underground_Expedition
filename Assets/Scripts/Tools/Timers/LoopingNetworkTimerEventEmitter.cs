using Fusion;
using UnityEngine;
using Events;

namespace Tools.Timers
{
    public class LoopingNetworkTimerEventEmitter : NetworkBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float interval = 3f;
        [SerializeField] private bool startOnSpawn = true;

        [Header("Event")]
        [SerializeField] private BoolEventChannel eventChannel;

        private NetworkTimer _timer;
        private bool _currentState;

        public override void Spawned()
        {
            _timer = GetComponent<NetworkTimer>();

            if (_timer == null)
                _timer = gameObject.AddComponent<NetworkTimer>();

            _timer.OnTimerCompleted += HandleTick;

            if (Object.HasStateAuthority && startOnSpawn)
            {
                _timer.StartTimer(interval);
            }
        }

        private void OnDestroy()
        {
            if (_timer != null)
                _timer.OnTimerCompleted -= HandleTick;
        }

        private void HandleTick()
        {
            if (!Object.HasStateAuthority) return;
            
            _currentState = !_currentState;
            
            eventChannel.RaiseEvent(_currentState);
            
            _timer.StartTimer(interval);
        }
    }
}