using System;
using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    public static LevelSystem instance;
    private int _actualLevel = 1;
    private int _skillPoints;
    
    public event Action<int> OnSkillPointsChanged;

    void Awake() { instance = this; }

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
            OnSkillPointsChanged?.Invoke(_skillPoints);
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
