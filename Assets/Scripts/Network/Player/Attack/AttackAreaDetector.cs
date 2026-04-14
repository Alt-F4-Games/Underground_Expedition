using UnityEngine;
using Fusion;

namespace Health
{
    public class AttackAreaDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private Vector3 _halfExtents = new Vector3(1.5f, 1f, 1.5f);
        [SerializeField] private Transform _center;
        [SerializeField] private LayerMask _hitLayers;

        [Header("Wall Check")]
        [SerializeField] private LayerMask _obstacleLayers;
        [SerializeField] private Transform _rayOrigin; 

        [Header("Offset Settings")]
        [SerializeField] private float _forwardOffsetMultiplier = 1f;

        [Header("Debug")]
        [SerializeField] private bool _drawGizmos = true;

        // ============================================================
        // PUBLIC API
        // ============================================================

        public NetworkObject GetClosestTarget(Vector3 attackerPosition)
        {
            Vector3 realCenter = GetOffsetCenter();

            Collider[] hits = Physics.OverlapBox(
                realCenter,
                _halfExtents,
                _center.rotation,
                _hitLayers
            );

            float closestDistance = float.MaxValue;
            NetworkObject closestTarget = null;

            foreach (var hit in hits)
            {
                var damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null) continue;

                var netObj = hit.GetComponentInParent<NetworkObject>();
                if (netObj == null) continue;

                if (netObj.transform == transform.root) continue;

                if (IsBlockedByWall(netObj)) continue;

                float distance = Vector3.Distance(attackerPosition, netObj.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = netObj;
                }
            }

            return closestTarget;
        }

        // ============================================================
        // WALL CHECK
        // ============================================================

        private bool IsBlockedByWall(NetworkObject target)
        {
            if (_rayOrigin == null) return false;

            Vector3 origin = _rayOrigin.position;
            Vector3 direction = (target.transform.position - origin).normalized;
            float distance = Vector3.Distance(origin, target.transform.position);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, _obstacleLayers))
            {
                if (hit.transform.root != target.transform)
                {
                    Debug.DrawRay(origin, direction * distance, Color.red, 0.2f);
                    return true;
                }
            }

            Debug.DrawRay(origin, direction * distance, Color.green, 0.2f);
            return false;
        }

        // ============================================================
        // INTERNAL
        // ============================================================

        private Vector3 GetOffsetCenter()
        {
            if (_center == null) return transform.position;

            return _center.position + _center.forward * (_halfExtents.z * _forwardOffsetMultiplier);
        }

        // ============================================================
        // GIZMOS
        // ============================================================

        private void OnDrawGizmos()
        {
            if (!_drawGizmos || _center == null) return;

            Vector3 realCenter = GetOffsetCenter();

            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(realCenter, _center.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, _halfExtents * 2);
        }
    }
}