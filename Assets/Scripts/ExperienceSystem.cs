using UnityEngine;

public class ExperienceSystem 
{
    public event Action OnLevelUp;
    
    private int _currentExp;
    private int _maxExp;
    
    public ExperienceSystem(int initialMaxXP)
    {
        _currentExp= 0;
        _maxExp = initialMaxXP;
    }

    public void AddXP(int amount)
    {
        _currentExp += amount;

        while (_currentExp >= _maxExp) 
        {
            _currentExp -= _maxExp;
            OnLevelUp?.Invoke();
        }
    }

    public int GetCurrentXP() => _currentExp;
    public int GetMaxXP() => _maxExp; 
}
