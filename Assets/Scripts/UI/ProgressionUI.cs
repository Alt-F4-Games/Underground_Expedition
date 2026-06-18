using Fusion;
using Local.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ProgressionUI : MonoBehaviour
    {
        public static ProgressionUI Instance;
        
        [Header("UI")]
        [SerializeField] private Image expFillImage;
        [SerializeField] private TextMeshProUGUI expText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI skillPointsText;

        private NetworkExperienceSystem _experienceSystem;
        private NetworkLevelSystem _levelSystem;

        private void Awake()
        {
            Instance = this;
        }

        public void RegisterPlayer(NetworkBehaviour player)
        {
            _experienceSystem = player.GetComponent<NetworkExperienceSystem>();
            _levelSystem = player.GetComponent<NetworkLevelSystem>();

            if (_experienceSystem == null || _levelSystem == null)
            {
                Debug.LogError("No se encontraron sistemas en el player");
                return;
            }

            _experienceSystem.OnExperienceGained += UpdateExperienceUI;
            _experienceSystem.OnLevelUp += UpdateLevelUI;
            _levelSystem.OnSkillPointsChanged += UpdateSkillPointsUI;

            RefreshAll();
        }

        private void RefreshAll()
        {
            UpdateExperienceUI(_experienceSystem.GetCurrentXp());
            UpdateLevelUI(_experienceSystem.GetLevel());
            UpdateSkillPointsUI(_levelSystem.GetSkillPoints());
        }

        private void UpdateExperienceUI(int currentXp)
        {
            int maxXp = _experienceSystem.GetMaxExp();

            float fill = (float)currentXp / maxXp;
            expFillImage.fillAmount = fill;

            expText.text = $"{currentXp} / {maxXp}";
        }

        private void UpdateLevelUI(int level)
        {
            levelText.text = $"Lvl {level}";

            UpdateExperienceUI(_experienceSystem.GetCurrentXp());
        }

        private void UpdateSkillPointsUI(int points)
        {
            skillPointsText.text = $"SP: {points}";
        }
    }
}