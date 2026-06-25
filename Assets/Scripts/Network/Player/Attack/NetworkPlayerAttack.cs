using Events;
using Fusion;
using Tools.EventSystem;
using UI;
using UnityEngine;
using Skills.Core;
using Network;

namespace Health
{
    public class NetworkPlayerAttack : NetworkBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] private int _damage = 10;
        [SerializeField] private float _attackCooldown = 0.5f;

        [Header("References")]
        [SerializeField] private AttackAreaDetector _detector;
        
        private PlayerSkillManager _skillManager;
        private PlayerStatsManager _statsManager;
        private NetworkPlayerController _playerController;

        private float _lastAttackTime;
        private NetworkButtons _previousButtons;
        private TickTimer _attackCooldownTimer;

        private void OnEnable() { EventController.Instance.AddListener<PlayerStatsEvent>(IncreaseAttack); }
        private void OnDisable() { EventController.Instance.RemoveListener<PlayerStatsEvent>(IncreaseAttack); }

        public override void Spawned()
        {
            _skillManager = GetComponent<PlayerSkillManager>();
            _statsManager = GetComponent<PlayerStatsManager>();
            _playerController = GetComponent<NetworkPlayerController>();
        }

        // ============================================================
        // INPUT (CLIENT ONLY)
        // ============================================================

        public override void FixedUpdateNetwork()
        {
            if (!HasInputAuthority)
                return;
            
            if (!GetInput(out NetworkInputPlayer input))
                return;

            NetworkButtons pressed = input.Buttons.GetPressed(_previousButtons);

            if (pressed.IsSet(NetworkInputPlayer.ATTACK_BUTTON))
            {
                TryAttack();
            }

            _previousButtons = input.Buttons;
        }

        private void TryAttack()
        {
            if (!_attackCooldownTimer.ExpiredOrNotRunning(Runner))
                return;

            _attackCooldownTimer = TickTimer.CreateFromSeconds(Runner, _attackCooldown);
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
                _playerController?.PlayMissAttackSound();
                _playerController?.PlayAttackAnimation();
                return;
            }

            var health = target.GetComponent<NetworkHealthSystem>();

            if (health)
            {
                // Fetch the dynamic multiplier from the Stats Manager (default to 1f if null)
                float currentMultiplier = _statsManager ? _statsManager.DamageMultiplier : 1f;
                
                // Calculate base damage considering active buffs
                int baseCalculatedDamage = Mathf.RoundToInt(_damage * currentMultiplier);
                
                bool empoweredAttack = _skillManager && _skillManager.IsEmpoweredAttackActive();
                
                int finalDamage = baseCalculatedDamage;
                
                if (_skillManager)
                {
                    finalDamage = _skillManager.GetModifiedDamage(finalDamage);
                }
                
                if (empoweredAttack)
                {
                    _playerController?.PlayEmpoweredAttackAnimation();
                    _playerController?.PlayEmpoweredAttackSound();
                }
                else
                {
                    _playerController?.PlayAttackAnimation();
                    _playerController?.PlayAttackSound();
                }

                health.TakeDamage(finalDamage, Object.InputAuthority); 
            }
        }
    }
}