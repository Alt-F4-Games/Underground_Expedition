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
        
        // reference to the Skill Manager for loose coupling
        private PlayerSkillManager _skillManager;

        private float _lastAttackTime;
        
        private void OnEnable() { EventController.Instance.AddListener<PlayerStatsEvent>(IncreaseAttack); }
        private void OnDisable() { EventController.Instance.RemoveListener<PlayerStatsEvent>(IncreaseAttack); }

        public override void Spawned()
        {
            // Cache the Skill Manager located on the same Player Prefab
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
                int finalDamage = _damage;
                
                if (_skillManager != null)
                {
                    finalDamage = _skillManager.GetModifiedDamage(finalDamage);
                }

                Debug.Log($"[SERVER] Applying {finalDamage} damage to: {target.name}");
                health.TakeDamage(finalDamage, Object.InputAuthority); 
            }
        }
    }
}