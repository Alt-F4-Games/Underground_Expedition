using Unity.VisualScripting;
using UnityEngine;
using System;

public class ExperienceSystem : MonoBehaviour
{
    public event Action<int> OnExperienceGained;
    public event Action<int> OnLevelUp;
    public static ExperienceSystem instance;

    private int _currentExp;
    private int _newLevel;
    private float _baseExp = 100;
    [SerializeField] private float _porcentageExp;
    
    void Awake() { instance = this; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddXP(10);
        }
    }
    
    public void AddXP(int amount)
    {
        _currentExp += amount;
        Debug.Log("Exp added:" + amount);
        if (_currentExp >= _baseExp)
        { 
            OnLevelUp?.Invoke(_newLevel++);
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

    private void IncrementBaseExp()
    {
        float factor = 1f + (_porcentageExp / 100f);
        _baseExp = Mathf.RoundToInt(_baseExp * factor);
        _baseExp = Mathf.CeilToInt(_baseExp / 10f) * 10;
    }
    public int GetCurrentXP() => _currentExp;
}
