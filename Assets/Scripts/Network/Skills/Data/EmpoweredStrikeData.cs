using UnityEngine;

namespace Skills
{
    [CreateAssetMenu(fileName = "EmpoweredStrikeData", menuName = "Skills/Empowered Strike Data")]
    public class EmpoweredStrikeData : SkillData
    {
        [Header("Empowered Strike Settings")]
        public int BaseCharges = 5;
        
        [Tooltip("Base damage increase (e.g., 0.10 for 10%)")]
        public float BaseDamageBonus = 0.10f; 
        
        [Tooltip("Additional growth per level (e.g., 0.02 for 2%)")]
        public float BonusPerLevel = 0.02f;
        
        public float GetTotalMultiplier(int currentLevel)
        {
            int levelMultiplier = Mathf.Max(0, currentLevel - 1);
            
            float totalBonus = BaseDamageBonus + (BonusPerLevel * levelMultiplier);
            
            return 1f + totalBonus;
        }
    }
}