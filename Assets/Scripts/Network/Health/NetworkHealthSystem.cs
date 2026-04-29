using System;
using Fusion;
using UnityEngine;
namespace Health
{
    public class NetworkHealthSystem : NetworkBehaviour, IDamageable
    { 
        [Header("Stats")]
        [SerializeField] private int _maxHealth = 100;

        [Networked] public int CurrentHealth { get; protected set; }
        [Networked] public bool IsAlive { get; set; }
        
        public Action OnDamageTaken;
        public Action<int> OnDamageFeedback;

        public int MaxHealth => _maxHealth;

        // ============================================================
        // Initialization
        // ============================================================

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                CurrentHealth = _maxHealth;
                IsAlive = true;
            }

            Debug.Log($"[Health] Spawned with HP: {CurrentHealth}");
        }

        // ============================================================
        // DAMAGE REQUEST (CLIENT → SERVER)
        // ============================================================

        public virtual void TakeDamage(int damage, PlayerRef playerRef =  default)
        {
            if (!HasStateAuthority) return;

            ApplyDamage(damage);
            
            OnDamageTaken?.Invoke();
            
            RPC_OnDamageFeedback(damage);
        }

        // ============================================================
        // HEAL REQUEST (CLIENT → SERVER)
        // ============================================================

        public void Heal(int heal)
        {
            if (!HasStateAuthority) return;

            ApplyHeal(heal);
        }
        
        // ============================================================
        // SERVER LOGIC
        // ============================================================

        private void ApplyDamage(int damage)
        {
            if (damage <= 0)
            {
                Debug.LogError("Damage amount cannot be negative or zero");
                return;
            }

            if (!IsAlive)
            {
                Debug.Log($"[SERVER] {gameObject.name} is already dead");
                return;
            }

            int previousHealth = CurrentHealth;

            CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);

            Debug.Log($"[SERVER] {gameObject.name} took {damage} damage. HP: {previousHealth} -> {CurrentHealth}");

            if (CurrentHealth <= 0)
            {
                Death();
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_OnDamageFeedback(int damage)
        {
            if (!Object.HasInputAuthority) return;

            OnDamageFeedback?.Invoke(damage);
        }
        
        private void ApplyHeal(int heal)
        {
            if (heal <= 0)
            {
                Debug.LogError("Heal amount cannot be negative or zero");
                return;
            }

            if (!IsAlive) return;

            CurrentHealth = Mathf.Min(CurrentHealth + heal, _maxHealth);

            Debug.Log($"{gameObject.name} healed {heal}. HP: {CurrentHealth}");
        }
        
        
        // ============================================================
        // DEATH (Server)
        // ============================================================

        protected virtual void Death()
        {
            if (!IsAlive) return;

            IsAlive = false;

            Debug.Log($"{gameObject.name} has died.");
        }

        // ============================================================
        // REVIVE (Server)
        // ============================================================

        public void Revive()
        {
            if (!HasStateAuthority) return;

            IsAlive = true;
            CurrentHealth = _maxHealth;

            Debug.Log($"{gameObject.name} has revived.");
        }
    }
}