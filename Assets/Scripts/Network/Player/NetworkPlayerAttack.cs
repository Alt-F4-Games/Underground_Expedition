using Fusion;
using UI;
using UnityEngine;

namespace Health
{
    public class NetworkPlayerAttack : NetworkBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] private int _damage = 10;
        [SerializeField] private float _attackDistance = 3f;
        [SerializeField] private float _attackCooldown = 0.5f;
        [SerializeField] private LayerMask _hitLayers;

        [Header("References")]
        [SerializeField] private Transform _cameraTransform;

        private float _lastAttackTime;

        // ============================================================
        // Update (local input)
        // ============================================================

        private void Update()
        {
            if (!HasInputAuthority) return;

            if (InputManager.Mode == InputMode.Game)
            {
                if (Input.GetMouseButtonDown(0)) 
                {
                    TryAttack();
                }
            }
            
        }

        // ============================================================
        // Attack Request
        // ============================================================

        private void TryAttack()
        {
            if (Time.time - _lastAttackTime < _attackCooldown)
                return;

            _lastAttackTime = Time.time;

            Vector3 origin = _cameraTransform.position;
            Vector3 direction = _cameraTransform.forward;

            Debug.DrawRay(origin, direction * _attackDistance, Color.red, 1f);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, _attackDistance, _hitLayers))
            {
                Debug.Log($"[CLIENT] Hit detected: {hit.collider.name}");

                var damageable = hit.collider.GetComponentInParent<IDamageable>();

                if (damageable != null)
                {
                    Debug.Log($"[CLIENT] Damageable found: {hit.collider.name}");

                    NetworkObject target = hit.collider.GetComponentInParent<NetworkObject>();

                    if (target != null)
                    {
                        Debug.Log($"[CLIENT] Sending damage RPC to: {target.name}");

                        RPC_RequestDamage(target, _damage);
                    }
                }
                else
                {
                    Debug.Log("[CLIENT] Hit object is NOT damageable");
                }
            }
            else
            {
                Debug.Log("[CLIENT] Raycast missed");
            }
        }

        // ============================================================
        // RPC → Server
        // ============================================================

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestDamage(NetworkObject target, int damage)
        {
            if (target == null) return;

            var health = target.GetComponent<NetworkHealthSystem>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }
}