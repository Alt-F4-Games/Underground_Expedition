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
            if (IsActive)
            {
                if (IsPlayerInside())
                    return;
            }
            
            _renderer.enabled = IsActive;
            _collider.enabled = IsActive;
        }
        
        private bool IsPlayerInside()
        {
            Collider[] hits = Physics.OverlapBox(
                transform.position,
                transform.localScale / 2f
            );

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                    return true;
            }

            return false;
        }
    }
}