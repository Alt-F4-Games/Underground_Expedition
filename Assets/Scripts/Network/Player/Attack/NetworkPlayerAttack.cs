using Fusion;
using UI;
using UnityEngine;

namespace Health
{
    public class NetworkPlayerAttack : NetworkBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] private int _damage = 10;
        [SerializeField] private float _attackCooldown = 0.5f;

        [Header("References")]
        [SerializeField] private AttackAreaDetector _detector;

        private float _lastAttackTime;

        // ============================================================
        // INPUT (CLIENT ONLY)
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

        private void TryAttack()
        {
            if (Time.time - _lastAttackTime < _attackCooldown)
                return;

            _lastAttackTime = Time.time;

            // 🔥 SOLO PEDIMOS AL SERVER QUE PROCESE
            RPC_RequestAttack();
        }

        // ============================================================
        // SERVER LOGIC
        // ============================================================

        // ReSharper disable Unity.PerformanceAnalysis
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestAttack()
        {
            if (!HasStateAuthority) return;

            Vector3 attackerPosition = transform.position;

            NetworkObject target = _detector.GetClosestTarget(attackerPosition);

            if (!target)
            {
                Debug.Log("[SERVER] No targets in area");
                return;
            }

            var health = target.GetComponent<NetworkHealthSystem>();

            if (health)
            {
                Debug.Log($"[SERVER] Applying damage to: {target.name}");
                health.TakeDamage(_damage, Object.InputAuthority);
            }
        }
    }
}