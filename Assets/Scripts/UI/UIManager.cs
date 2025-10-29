using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance {get; private set;}

    [Header("Experience")]
    [SerializeField] private TextMeshProUGUI _experienceText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Slider experienceSlider;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void ShowScreen(GameObject screenToShow)
    {
        screenToShow.SetActive(true);   
    }

    public void HideScreen(GameObject screenToHide)
    {
        screenToHide.SetActive(false);
    }

    private void OnEnable()
    {
        if (ExperienceSystem.instance != null)
        {
            ExperienceSystem.instance.OnExperienceGained += HandleExperienceGained;
            ExperienceSystem.instance.OnLevelUp += HandleLevelUp;
        }
        else
        {
            Debug.LogWarning("UIManager: No se pudo suscribir a ExperienceSystem. Instancia no encontrada.");
        }
    }

    private void OnDisable()
    {
        if (ExperienceSystem.instance != null)
        {
            ExperienceSystem.instance.OnExperienceGained -= HandleExperienceGained;
            ExperienceSystem.instance.OnLevelUp -= HandleLevelUp;
        }
    }
    
    private void HandleLevelUp(int newLevel)
    {
        _levelText.text = "NIVEL: " + newLevel.ToString();
    }
    
    private void HandleExperienceGained(int currentExp)
    {
        float maxExp = ExperienceSystem.instance.GetMaxExp();
        
        if (maxExp > 0)
        {
            experienceSlider.value = (float)currentExp / maxExp;
        }
        
        if (_experienceText != null)
        {
            _experienceText.text = currentExp.ToString() + " / " + maxExp.ToString();
        }
    }
}
