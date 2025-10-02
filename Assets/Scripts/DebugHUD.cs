using UnityEngine;
using TMPro;

public class DebugHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthSystem playerHealth;       // Referencia al sistema de vida
    [SerializeField] private ExperienceSystem experienceSys;  // Referencia al sistema de experiencia
    [SerializeField] private LevelSystem levelSys;            // Referencia al sistema de niveles

    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI levelText;

    private void Update()
    {
        if (playerHealth != null)
        {
            healthText.text = $"Vida: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}";
        }

        if (experienceSys != null)
        {
            expText.text = $"Experiencia: {experienceSys.GetCurrentXP()}";
        }

        if (levelSys != null)
        {
            levelText.text = $"Nivel: {levelSys.GetLevel()}";
        }
    }
}
