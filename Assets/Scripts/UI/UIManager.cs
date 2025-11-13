using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    [Header("Experience")]
    [SerializeField] private TextMeshProUGUI _experienceText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Image experienceRadialFill;
    [SerializeField] private TextMeshProUGUI _skillPointsText;
    
    [Header("Health")]
    [SerializeField] private Slider _healthSlider;
    private HealthSystem _playerHealth;

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

    public void SetExperienceSystems(ExperienceSystem expSystem, LevelSystem lvlSystem)
    {
        if (ExperienceSystem.instance != null)
        {
            ExperienceSystem.instance.OnExperienceGained -= HandleExperienceGained;
            ExperienceSystem.instance.OnLevelUp -= HandleLevelUp;
        }

        if (LevelSystem.instance != null)
        {
            LevelSystem.instance.OnSkillPointsChanged -= HandleSkillPointsChanged;
        }

        if (expSystem != null)
        {
            expSystem.OnExperienceGained += HandleExperienceGained;
            expSystem.OnLevelUp += HandleLevelUp;

            HandleExperienceGained(expSystem.GetCurrentXP());
            HandleLevelUp(lvlSystem.GetLevel());
        }

        if (lvlSystem != null)
        {
            lvlSystem.OnSkillPointsChanged += HandleSkillPointsChanged;
            HandleSkillPointsChanged(lvlSystem.GetSkillPoints());
        }
    }

    public void SetPlayer(GameObject player)
    {
        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged -= HandleHealthChanged;
        }

        _playerHealth = player.GetComponent<HealthSystem>();

        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
        }
    }

    private void HandleLevelUp(int newLevel)
    {
        _levelText.text = newLevel.ToString();
    }
    
    private void HandleExperienceGained(int currentExp)
    {
        float maxExp = ExperienceSystem.instance.GetMaxExp();
        float normalizedExp = maxExp > 0 ? currentExp / maxExp : 0f;

        if (experienceRadialFill != null)
        {
            experienceRadialFill.fillAmount = normalizedExp;
        }

        if (_experienceText != null)
        {
            _experienceText.text = $"{currentExp} / {maxExp}";
        }
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if (_healthSlider == null) return;
        _healthSlider.value = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
    }

    private void HandleSkillPointsChanged(int currentSkillPoints)
    {
        _skillPointsText.text = currentSkillPoints.ToString();
    }
}

