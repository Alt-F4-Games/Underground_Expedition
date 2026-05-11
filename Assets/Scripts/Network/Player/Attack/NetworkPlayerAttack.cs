using Events;
using Fusion;
using UI;
using UnityEngine;
using Skills.Core;

namespace Health
{
    public class NetworkPlayerAttack : NetworkBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] private int _damage = 10;
        [SerializeField] private float _attackCooldown = 0.5f;

        [Header("References")]
        [SerializeField] private AttackAreaDetector _detector;
        
        // Generic reference to the Skill Manager for loose coupling (Your code)
        private PlayerSkillManager _skillManager;

        private float _lastAttackTime;

        // Integration with Stats System (from develop)
        private void OnEnable() { EventController.Instance.AddListener<PlayerStatsEvent>(IncreaseAttack); }
        private void OnDisable() { EventController.Instance.RemoveListener<PlayerStatsEvent>(IncreaseAttack); }

        public override void Spawned()
        {
            // Cache the Skill Manager located on the same Player Prefab (Your code)
            _skillManager = GetComponent<PlayerSkillManager>();
        }

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
            
            RPC_RequestAttack();
        }

        // Feature from develop: Permanently increase base damage via events
        private void IncreaseAttack(PlayerStatsEvent evt)
        {
            if (!HasStateAuthority) return;
            _damage += evt.PlayerDamage;
        }

        // ============================================================
        // SERVER LOGIC
        // ============================================================

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
                // 1. We start with the base damage (which might have been increased by events)
                int finalDamage = _damage;

                // 2. We apply skill modifiers polymorphically (Your architecture)
                if (_skillManager != null)
                {
                    finalDamage = _skillManager.GetModifiedDamage(finalDamage);
                }

                Debug.Log($"[SERVER] Applying {finalDamage} damage to: {target.name}");

                // 3. Apply the final damage passing the InputAuthority for attribution (from develop)
                health.TakeDamage(finalDamage, Object.InputAuthority); 
            }
        }
    }
}