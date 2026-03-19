using Fusion;
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

        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
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

        if (Physics.Raycast(
                _cameraTransform.position,
                _cameraTransform.forward,
                out RaycastHit hit,
                _attackDistance,
                _hitLayers))
        {
            var damageable = hit.collider.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                NetworkObject target = hit.collider.GetComponentInParent<NetworkObject>();

                if (target != null)
                {
                    RPC_RequestDamage(target, _damage);
                }
            }
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