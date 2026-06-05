using Fusion;
using UnityEngine;

namespace Skills
{
    public abstract class NetworkSkill : NetworkBehaviour
    {
        [Header("Immutable Data")]
        [SerializeField] protected SkillData _skillData; 
        
        // Expose data so UI can read the Icon and Name
        public SkillData Data => _skillData;
        
        [Networked] public int CurrentLevel { get; protected set; }
        [Networked] public TickTimer CooldownEnd { get; protected set; }
        
        // Timer for the execution/active duration (Clockwise light overlay)
        [Networked] public TickTimer ActiveEnd { get; protected set; }

        public override void Spawned()
        {
            base.Spawned();
            
            // Skills start unlocked at level 1 by default.
            if (HasStateAuthority && CurrentLevel == 0)
            {
                CurrentLevel = 1;
            }
        }

        public virtual bool CanCast(NetworkRunner runner)
        {
            if (CurrentLevel <= 0) return false;
            if (!CooldownEnd.ExpiredOrNotRunning(runner)) return false;
            if (!ActiveEnd.ExpiredOrNotRunning(runner)) return false; // Prevent cast if already active
            
            return true;
        }

        public abstract void OnExecute(NetworkRunner runner);

        protected virtual void StartCooldown(NetworkRunner runner)
        {
            if (_skillData == null) return;
            
            float cd = _skillData.GetCooldown(CurrentLevel);
            CooldownEnd = TickTimer.CreateFromSeconds(runner, cd);
        }

        public virtual void UpgradeSkill()
        {
            if (!HasStateAuthority) return;

            // Server-side validation against MaxLevel
            if (_skillData != null && CurrentLevel < _skillData.MaxLevel)
            {
                CurrentLevel++;
            }
        }
        
        public virtual int ModifyAttackDamage(int baseDamage)
        {
            return baseDamage;
        }

        // ============================================================
        // UI HOOKS (Polymorphism for decoupled UI)
        // ============================================================
        
        /// <summary>
        /// Returns current charges. Returns -1 if the skill doesn't use charges.
        /// </summary>
        public virtual int GetCurrentCharges() => -1;

        /// <summary>
        /// Returns maximum charges. Returns -1 if the skill doesn't use charges.
        /// </summary>
        public virtual int GetMaxCharges() => -1;

        /// <summary>
        /// Returns a normalized value (0 to 1) representing the active execution progress.
        /// </summary>
        public virtual float GetActiveProgress(NetworkRunner runner)
        {
            // By default, skills don't have an active duration progress.
            // Specific skills can override this if they have a sustained effect.
            return 0f;
        }
    }
}