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
        [Networked] public int MaxHealth { get; protected set; }
        
        public Action OnDamageTaken;
        public Action<int> OnDamageFeedback;
        
        // ============================================================
        // Initialization
        // ============================================================

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                MaxHealth = _maxHealth;
                CurrentHealth = _maxHealth;
                IsAlive = true;
            }
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
                return;
            }

            if (!IsAlive)
            {
                return;
            }
            
            CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
            
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
                return; 

            if (!IsAlive) return;

            CurrentHealth = Mathf.Min(CurrentHealth + heal, _maxHealth);
        }
        
        
        // ============================================================
        // DEATH (Server)
        // ============================================================

        protected virtual void Death()
        {
            if (!IsAlive) return;

            IsAlive = false;
        }

        // ============================================================
        // REVIVE (Server)
        // ============================================================

        public void Revive()
        {
            if (!HasStateAuthority) return;

            IsAlive = true;
            CurrentHealth = _maxHealth;
        }
    }
}