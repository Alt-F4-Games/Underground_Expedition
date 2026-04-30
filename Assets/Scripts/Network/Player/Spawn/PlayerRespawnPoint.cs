using Fusion;
using UnityEngine;

namespace Network.Spawn
{
    public class PlayerRespawnPoint : NetworkBehaviour
    {
        [Header("Settings")]
        public int order;

        [SerializeField] private Transform _spawnPoint;

        [Networked] public bool IsActive { get; set; }
        [Networked] public bool WasActivated { get; set; }

        [Header("Debug")]
        public bool drawGizmos = true;

        public Vector3 GetPosition()
        {
            return _spawnPoint != null ? _spawnPoint.position : transform.position;
        }

        public override void Render()
        {
            if (_spawnPoint == null)
                return;

            Debug.DrawLine(
                _spawnPoint.position,
                _spawnPoint.position + Vector3.up * 2f,
                IsActive ? Color.cyan : Color.red
            );
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            if (_spawnPoint == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_spawnPoint.position, 0.3f);
        }
    }
}