using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    private int _upgradeLevel = 1;
    
    void Start()
    {
        ExperienceSystem.instance.OnLevelUp += UpdateLevel;
        Debug.Log("Level:" +  _upgradeLevel);
    }

    void UpdateLevel(int newLevel)
    {
        _upgradeLevel += newLevel;
        Debug.Log("Upgrade level:" + _upgradeLevel);
    } 
}
