using Unity.VisualScripting;
using UnityEngine;
using System;

public class ExperienceSystem : MonoBehaviour
{
    public event Action<int> OnExperienceGained;
    public event Action<int> OnLevelUp;
    public static ExperienceSystem instance;

    private int _currentExp;
    private int _newLevel = 1;
    private float _baseExp = 100;
    [SerializeField] private float _porcentageExp;
    [SerializeField] private float _maxLevel;
    
    void Awake() { instance = this; Debug.Log("Level:" + _newLevel); }
    
    public void AddXP(int amount)
    {
        if(_maxLevel > _newLevel )
        {
            _currentExp += amount;
            Debug.Log("Exp added:" + amount);
            
                if (_currentExp >= _baseExp)
                { 
                    _newLevel++;
                    OnLevelUp?.Invoke(_newLevel);
                    IncrementBaseExp();
                    Debug.Log("Actual Exp:" + _currentExp);
                    Debug.Log("Max exp:" + _baseExp);
                    _currentExp = 0;
                   
                }else if (_baseExp > _currentExp)
                {
                    Debug.Log("Actual Exp:" + _currentExp);
                    OnExperienceGained?.Invoke(_currentExp);
                }
        }
        else
        {
            Debug.Log("Max Level reached");
            return;
        }    
    }

    private void IncrementBaseExp()
    {
        float factor = 1f + (_porcentageExp / 100f);
        _baseExp = Mathf.RoundToInt(_baseExp * factor);
        _baseExp = Mathf.CeilToInt(_baseExp / 10f) * 10;
    }
    public int GetCurrentXP() => _currentExp;
}
