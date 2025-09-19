using UnityEngine;

public class ExperienceSystem : MonoBehaviour
{
    private int _currentExp;
    [SerializeField] private float _maxExp;
    [SerializeField] private float _porcentageExp = 50f;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _currentExp += 10;
            Debug.Log($"Experiencia aumentada:{_currentExp}");
        }

        if (_currentExp >= _maxExp)
        {
            LevelUp();
            _currentExp = 0;
        }
    }

    private void LevelUp()
    {
        float factor = 1 + (_porcentageExp / 100f);
        _maxExp *= factor;
        Debug.Log($"Experiencia maxima renovada:{_maxExp}");
    }    
}
