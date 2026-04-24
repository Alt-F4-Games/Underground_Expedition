using UnityEngine;

namespace Network.Environment.ToggleableObjects.Implementations
{
    public class EventToggleableDoor : NetworkEventToggleable
    {
        private MeshRenderer _renderer;
        private Collider _collider;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _collider = GetComponent<Collider>();
        }

        protected override void ApplyState()
        {
            _renderer.enabled = IsActive;
            _collider.enabled = IsActive;
        }
    }
}