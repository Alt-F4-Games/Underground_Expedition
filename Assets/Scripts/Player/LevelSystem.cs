using System;
using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    private int _actualLevel = 1;
    private int _skillPoints;
    
    public event Action<int, int> OnSkillPointsChanged;
    void Start()
    {
        ExperienceSystem.instance.OnLevelUp += UpdateLevel;
        Debug.Log("Level:" +  _actualLevel);
    }

   void UpdateLevel(int newLevel)
    {
        _actualLevel = newLevel;
        Debug.Log("Upgrade level:" + _actualLevel);
        if (_actualLevel == 3 || _actualLevel == 6 || _actualLevel == 8)
        {
            _skillPoints++;
            OnSkillPointsChanged?.Invoke(_skillPoints, _skillPoints);
            Debug.Log("Skill points:" + _skillPoints);
        }
    }
     private void OnDestroy()
        {
            ExperienceSystem.instance.OnLevelUp -= UpdateLevel;
        }
     
     public int GetLevel() => _actualLevel;
     public int GetSkillPoints() => _skillPoints;

}
