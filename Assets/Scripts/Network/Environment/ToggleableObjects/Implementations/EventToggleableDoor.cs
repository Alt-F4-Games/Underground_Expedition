using UnityEngine;
using UnityEngine.AI;

namespace Network.Environment.ToggleableObjects.Implementations
{
    public class EventToggleableDoor : NetworkEventToggleable
    {
        private NavMeshObstacle _obstacle;
        private MeshRenderer _renderer;
        private Collider _collider;

        private void Awake()
        {
            _obstacle = GetComponent<NavMeshObstacle>();
            _renderer = GetComponent<MeshRenderer>();
            _collider = GetComponent<Collider>();
        }

        protected override void ApplyState()
        {
            _obstacle.enabled = IsActive;
            _renderer.enabled = IsActive;
            _collider.enabled = IsActive;
        }
    }
}