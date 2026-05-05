using Fusion;
using System;

namespace Tools.Timers
{
    public class NetworkTimer : NetworkBehaviour
    {
        [Networked] private TickTimer TickTimer { get; set; }
        [Networked] private bool IsPaused { get; set; }
        [Networked] private float RemainingTime { get; set; }

        public event Action OnTimerCompleted;

        public void StartTimer(float duration)
        {
            if (!Object.HasStateAuthority) return;

            TickTimer = TickTimer.CreateFromSeconds(Runner, duration);
            IsPaused = false;
        }

        public void Pause()
        {
            if (!Object.HasStateAuthority) return;

            if (TickTimer.IsRunning)
            {
                RemainingTime = TickTimer.RemainingTime(Runner) ?? 0f;
                TickTimer = default;
                IsPaused = true;
            }
        }

        public void Resume()
        {
            if (!Object.HasStateAuthority) return;

            if (IsPaused)
            {
                TickTimer = TickTimer.CreateFromSeconds(Runner, RemainingTime);
                IsPaused = false;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (TickTimer.IsRunning && TickTimer.Expired(Runner))
            {
                TickTimer = default;
                OnTimerCompleted?.Invoke();
            }
        }

        public float GetRemainingTime()
        {
            if (IsPaused)
                return RemainingTime;

            return TickTimer.RemainingTime(Runner) ?? 0f;
        }

        public bool IsRunning()
        {
            return TickTimer.IsRunning;
        }
    }
}