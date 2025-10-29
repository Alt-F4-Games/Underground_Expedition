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
    [SerializeField] private Image experienceRadialFill;
    
    [Header("Health")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private HealthSystem _playerHealth;
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
        
        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
        }
        else
        {
            Debug.LogWarning("UIManager: No hay referencia al HealthSystem del jugador.");
        }
    }

    private void OnDisable()
    {
        if (ExperienceSystem.instance != null)
        {
            ExperienceSystem.instance.OnExperienceGained -= HandleExperienceGained;
            ExperienceSystem.instance.OnLevelUp -= HandleLevelUp;
        }
        
        if (_playerHealth != null){_playerHealth.OnHealthChanged -= HandleHealthChanged;}
    }
    
    private void HandleLevelUp(int newLevel)
    {
        _levelText.text = newLevel.ToString();
    }
    
    private void HandleExperienceGained(int currentExp)
    {
        float maxExp = ExperienceSystem.instance.GetMaxExp();
        float normalizedExp = 0f;

        if (maxExp > 0)
        {
            normalizedExp = (float)currentExp / maxExp;
        }
        
        if (experienceRadialFill != null)
        {
            experienceRadialFill.fillAmount = normalizedExp;
        }
        
        
        if (_experienceText != null)
        {
            _experienceText.text = currentExp.ToString() + " / " + maxExp.ToString();
        }
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth) {}
}
