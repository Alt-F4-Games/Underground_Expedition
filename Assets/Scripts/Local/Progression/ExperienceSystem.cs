using System;
using UnityEngine;

namespace Player
{
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
            if (_newLevel >= _maxLevel)
            {
                if (_currentExp != 0)
                {
                    _currentExp = 0;
                    OnExperienceGained?.Invoke(_currentExp);
                }
                Debug.Log("Max Level reached");
                return; 
            }
            _currentExp += amount;
            Debug.Log("Exp added:" + amount);
            OnExperienceGained?.Invoke(_currentExp);
        
            while (_currentExp >= _baseExp && _maxLevel > _newLevel)
            {
                int leftoverExp = _currentExp - (int)_baseExp;
        
                _newLevel++;
                OnLevelUp?.Invoke(_newLevel); 
                IncrementBaseExp();           
        
                _currentExp = leftoverExp;    
                OnExperienceGained?.Invoke(_currentExp);
        
                Debug.Log("Actual Exp:" + _currentExp);
                Debug.Log("Max exp:" + _baseExp);

                if (_maxLevel <= _newLevel)
                {
                    _currentExp = 0; 
                    OnExperienceGained?.Invoke(_currentExp); 
                    Debug.Log("Max Level reached");
                    return;
                }
            }
        }

        private void IncrementBaseExp()
        {
            float factor = 1f + (_porcentageExp / 100f);
            _baseExp = Mathf.RoundToInt(_baseExp * factor);
            _baseExp = Mathf.CeilToInt(_baseExp / 10f) * 10;
        }
        public int GetCurrentXP() => _currentExp;
        public float GetMaxExp() => _baseExp;
    }
}
