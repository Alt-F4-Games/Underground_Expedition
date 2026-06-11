using Events;
using Fusion;
using Tools.EventSystem;
using Health;
using UnityEngine;

namespace Network
{
    public class PlayerStatsManager : NetworkBehaviour
    {
        [Header("Base Stat Upgrades")]
        [SerializeField] private int _plusMaxHealth = 10;
        [SerializeField] private float _plusMaxStamimna = 5;
        [SerializeField] private int _plusPlayerDamage;
        
        private PlayerStatsEvent  _playerStatsEvent = new ();

        [Header("References")]
        private NetworkPlayerHealth _health;
        private NetworkPlayerController _controller;

        // ============================================================
        // NETWORKED BUFFS & TIMERS (Facade System)
        // ============================================================
        
        [Networked] public float DamageMultiplier { get; private set; }
        [Networked] public float WalkSpeedMultiplier { get; private set; }
        [Networked] public float SprintSpeedMultiplier { get; private set; }
        
        [Networked] private TickTimer _damageBuffTimer { get; set; }
        [Networked] private TickTimer _speedBuffTimer { get; set; }

        public override void Spawned()
        {
            _health = GetComponent<NetworkPlayerHealth>();
            _controller = GetComponent<NetworkPlayerController>();
            
            if (HasStateAuthority)
            {
                // Initialize multipliers to 1.0 (100% / normal state)
                DamageMultiplier = 1f;
                WalkSpeedMultiplier = 1f;
                SprintSpeedMultiplier = 1f;
            }
        }

        // ============================================================
        // UPDATE TIMERS
        // ============================================================
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;

            // Reset damage multiplier when the buff timer expires
            if (_damageBuffTimer.Expired(Runner))
            {
                DamageMultiplier = 1f;
                _damageBuffTimer = TickTimer.None;
            }

            // Reset speed multipliers when the buff timer expires
            if (_speedBuffTimer.Expired(Runner))
            {
                WalkSpeedMultiplier = 1f;
                SprintSpeedMultiplier = 1f;
                _speedBuffTimer = TickTimer.None;
            }
        }

        // ============================================================
        // PUBLIC API (For Consumables, Traps, and Skills)
        // ============================================================
        
        /// <summary>
        /// Restores a specific amount of health points up to the maximum.
        /// </summary>
        public void HealPlayer(int amount)
        {
            if (!HasStateAuthority || _health == null) return;
            
            // Call the public method on the health script instead of setting properties directly
            _health.Heal(amount);
        }

        /// <summary>
        /// Restores a specific amount of stamina up to the maximum.
        /// </summary>
        public void RestoreStamina(float amount)
        {
            if (!HasStateAuthority || _controller == null) return;
            
            _controller.CurrentStamina += amount;
            if (_controller.CurrentStamina > _controller.MaxStamina)
                _controller.CurrentStamina = _controller.MaxStamina;
        }

        /// <summary>
        /// Permanently increases the max health by a percentage of the current max health.
        /// </summary>
        public void ApplyMaxHealthBuff(float percentage)
        {
            if (!HasStateAuthority || _health == null) return;

            int bonus = Mathf.RoundToInt(_health.MaxHealth * percentage);
            // Call the public method on the health script
            _health.AddMaxHealth(bonus);
        }

        /// <summary>
        /// Applies a temporary damage multiplier.
        /// </summary>
        public void ApplyDamageBuff(float percentage, float duration)
        {
            if (!HasStateAuthority) return;
            DamageMultiplier = 1f + percentage;
            _damageBuffTimer = TickTimer.CreateFromSeconds(Runner, duration);
        }

        /// <summary>
        /// Applies a temporary speed multiplier (affects walk and sprint independently).
        /// </summary>
        public void ApplySpeedBuff(float walkPercentage, float sprintPercentage, float duration)
        {
            if (!HasStateAuthority) return;
            WalkSpeedMultiplier = 1f + walkPercentage;
            SprintSpeedMultiplier = 1f + sprintPercentage;
            _speedBuffTimer = TickTimer.CreateFromSeconds(Runner, duration);
        }

        // ============================================================
        // PERMANENT EVENTS (Level Up / Rewards)
        // ============================================================
        public void ApplyStatsServer()
        {
            if (!HasStateAuthority) return;

            _playerStatsEvent.MaxHealth =  _plusMaxHealth;
            _playerStatsEvent.MaxStamina =  _plusMaxStamimna;
            _playerStatsEvent.PlayerDamage =  _plusPlayerDamage;
                                        
            EventController.Instance.TriggerEvent(_playerStatsEvent);
        }
        
    }
}