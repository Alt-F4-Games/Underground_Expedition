using Fusion;

namespace Network.Environment.ToggleableObjects
{
    public abstract class NetworkToggleable : NetworkBehaviour, IToggleable
    {
        [Networked] protected NetworkBool IsActive { get; set; }

        private bool _lastState;

        public override void Spawned()
        {
            _lastState = IsActive;
            ApplyState();
        }

        public override void Render()
        {
            if (_lastState != IsActive)
            {
                _lastState = IsActive;
                ApplyState();
            }
        }

        public virtual void SetState(bool state)
        {
            if (Object.HasStateAuthority)
            {
                IsActive = state;
            }
        }

        public void Toggle()
        {
            SetState(!IsActive);
        }

        public bool GetState() => IsActive;

        protected abstract void ApplyState();
    }
}