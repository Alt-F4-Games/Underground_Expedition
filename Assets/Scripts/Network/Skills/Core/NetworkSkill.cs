using Fusion;
using UnityEngine;

namespace Skills
{
    public abstract class NetworkSkill : NetworkBehaviour
    {
        [Header("Immutable Data")]
        [SerializeField] protected SkillData _skillData; 
        
        [Networked] 
        public int CurrentLevel { get; protected set; }
        
        [Networked] 
        public TickTimer CooldownEnd { get; protected set; }

        public override void Spawned()
        {
            base.Spawned();
            
            // Temporary bypass for testing: Force level 1 since the progression system is not yet implemented.
            if (HasStateAuthority && CurrentLevel == 0)
            {
                CurrentLevel = 1;
            }
        }

        public virtual bool CanCast(NetworkRunner runner)
        {
            if (CurrentLevel <= 0) return false;
            
            if (!CooldownEnd.ExpiredOrNotRunning(runner)) return false;
            
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
            CurrentLevel++;
        }
        
        public virtual int ModifyAttackDamage(int baseDamage)
        {
            return baseDamage;
        }
    }
}