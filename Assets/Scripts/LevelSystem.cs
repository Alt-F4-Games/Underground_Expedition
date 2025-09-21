using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    private int _actualLevel = 1;
    
    void Start()
    {
        ExperienceSystem.instance.OnLevelUp += UpdateLevel;
        Debug.Log("Level:" +  _actualLevel);
    }

   void UpdateLevel(int newLevel)
    {
        _actualLevel = newLevel;
        Debug.Log("Upgrade level:" + _actualLevel);
    }
     private void OnDestroy()
        {
            ExperienceSystem.instance.OnLevelUp -= UpdateLevel;
        }

}
