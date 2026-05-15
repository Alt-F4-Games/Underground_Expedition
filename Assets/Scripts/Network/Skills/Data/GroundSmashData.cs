using UnityEngine;

namespace Skills
{
    [CreateAssetMenu(fileName = "GroundSmashData", menuName = "Skills/Ground Smash Data")]
    public class GroundSmashData : SkillData
    {
        [Header("Ground Smash Settings")]
        public int BaseDamage = 20;
        public int DamagePerLevel = 5;
        
        [Header("Area Dimensions (Cylinder)")]
        public float Radius = 2f;
        public float Height = 1f;
        
        [Tooltip("Vertical adjustment to align the cylinder's base with the ground if the player's pivot is centered.")]
        public float VerticalOffset = -1f; 
        
        [Header("Mechanics")]
        public float SelfStunDuration = 0.5f;
        public LayerMask EnemyLayer;
        
        public int GetTotalDamage(int currentLevel)
        {
            int levelMultiplier = Mathf.Max(0, currentLevel - 1);
            return BaseDamage + (DamagePerLevel * levelMultiplier);
        }
        
        public override float GetCooldown(int currentLevel)
        {
            int levelMultiplier = Mathf.Max(0, currentLevel - 1);
            return BaseCooldown + (CooldownReductionPerLevel * levelMultiplier);
        }
    }
}