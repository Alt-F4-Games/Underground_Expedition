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
        
        public virtual bool CanCast(NetworkRunner runner)
        {
            if (CurrentLevel <= 0) return false;
            
            return CooldownEnd.ExpiredOrNotRunning(runner);
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
    }
}