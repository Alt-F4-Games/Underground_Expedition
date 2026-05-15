using UnityEngine;

namespace Skills
{
    public abstract class SkillData : ScriptableObject
    {
        [Header("Base Info")]
        public string SkillName;
        [TextArea] public string Description;
        public Sprite Icon;
        
        [Header("Progression Settings")]
        public int MaxLevel = 5;

        [Header("Cooldown Settings")]
        public float BaseCooldown;
        public float CooldownReductionPerLevel;
        
        public virtual float GetCooldown(int currentLevel)
        {
            int levelMultiplier = Mathf.Max(0, currentLevel - 1);
            return BaseCooldown + (CooldownReductionPerLevel * levelMultiplier);
        }
    }
}