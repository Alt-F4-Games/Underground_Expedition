using Fusion;
using UnityEngine;

namespace Network.Environment.ToggleableObjects
{
    public abstract class NetworkToggleable : NetworkBehaviour, IToggleable
    {
        [Header("Toggleable Settings")]
        [SerializeField] private bool initialState = false;

        [Networked] protected NetworkBool IsActive { get; set; }

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                IsActive = initialState;
            }

            ApplyState();
        }

        public override void Render()
        {
            ApplyState();
        }

        public virtual void SetState(bool state)
        {
            if (!Object.HasStateAuthority) return;

            if (IsActive == state) return;

            IsActive = state;
            
            ApplyState();
        }

        public void Toggle()
        {
            SetState(!IsActive);
        }

        public bool GetState() => IsActive;

        protected abstract void ApplyState();
    }
}