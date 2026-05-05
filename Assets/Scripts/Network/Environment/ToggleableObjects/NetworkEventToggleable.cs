using Events;
using UnityEngine;

namespace Network.Environment.ToggleableObjects
{
    public abstract class NetworkEventToggleable : NetworkToggleable
    {
        [Header("Event Channel")]
        [SerializeField] private BoolEventChannel eventChannel;

        [Header("Behavior")]
        [SerializeField] private bool invert;

        private void OnEnable()
        {
            if (eventChannel != null)
                eventChannel.OnEventRaised += HandleEvent;
        }

        private void OnDisable()
        {
            if (eventChannel != null)
                eventChannel.OnEventRaised -= HandleEvent;
        }

        private void HandleEvent(bool value)
        {
            if (!Object.HasStateAuthority) return;

            if (invert)
                value = !value;

            SetState(value);
        }
    }
}