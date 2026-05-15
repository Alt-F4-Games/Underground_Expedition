using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Skills;
using Skills.Core;
using Local.Progression;

namespace UI
{
    public class InventorySkillSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameAndLevelText;
        [SerializeField] private TextMeshProUGUI _statsText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _upgradeButton;

        private NetworkSkill _trackedSkill;
        private PlayerSkillManager _skillManager;
        private NetworkLevelSystem _levelSystem;
        private int _slotIndex;
        
        public void AssignSkill(NetworkSkill skill, PlayerSkillManager manager, NetworkLevelSystem levelSystem, int slotIndex)
        {
            _trackedSkill = skill;
            _skillManager = manager;
            _levelSystem = levelSystem;
            _slotIndex = slotIndex;

            if (_upgradeButton != null)
            {
                _upgradeButton.onClick.RemoveAllListeners();
                _upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }

            if (_levelSystem != null)
            {
                _levelSystem.OnSkillPointsChanged += UpdateUpgradeButtonVisibility;
            }

            UpdateUI();
        }

        private void OnDestroy()
        {
            if (_levelSystem != null)
            {
                _levelSystem.OnSkillPointsChanged -= UpdateUpgradeButtonVisibility;
            }
        }

        private void UpdateUpgradeButtonVisibility(int points)
        {
            if (_upgradeButton != null)
            {
                // Check max level condition
                bool isMaxLevel = _trackedSkill != null && _trackedSkill.Data != null && _trackedSkill.CurrentLevel >= _trackedSkill.Data.MaxLevel;
                
                // Only show the upgrade button if the player has points to spend AND is not max level
                _upgradeButton.gameObject.SetActive(points > 0 && !isMaxLevel);
            }
        }

        private void Update()
        {
            // Refresh data if the skill changes level (UI polling for the inventory panel)
            if (_trackedSkill != null && _trackedSkill.Object.IsValid && gameObject.activeInHierarchy)
            {
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (_trackedSkill == null || _trackedSkill.Data == null) return;

            _iconImage.sprite = _trackedSkill.Data.Icon;
            
            // Display "MAX" instead of the number if capped
            bool isMaxLevel = _trackedSkill.CurrentLevel >= _trackedSkill.Data.MaxLevel;
            string levelString = isMaxLevel ? "MAX" : _trackedSkill.CurrentLevel.ToString();
            
            _nameAndLevelText.text = $"{_trackedSkill.Data.SkillName} - Lvl {levelString}";
            _descriptionText.text = _trackedSkill.Data.Description;

            // Dynamic stat display based on specific SkillData types
            string stats = "";
            
            // Assume GroundSmashData has a damage calculation method
            if (_trackedSkill.Data is GroundSmashData smashData) 
            {
                stats = $"ATK: {smashData.GetTotalDamage(_trackedSkill.CurrentLevel)}  CD: {smashData.GetCooldown(_trackedSkill.CurrentLevel):F1}s";
            }
            // Assume EmpoweredStrikeData shows its multiplier as percentage
            else if (_trackedSkill.Data is EmpoweredStrikeData strikeData) 
            {
                float bonus = (strikeData.GetTotalMultiplier(_trackedSkill.CurrentLevel) - 1f) * 100f;
                stats = $"BONUS: +{bonus:F0}%  CD: {strikeData.GetCooldown(_trackedSkill.CurrentLevel):F1}s";
            }
            
            _statsText.text = stats;

            if (_levelSystem != null) UpdateUpgradeButtonVisibility(_levelSystem.GetSkillPoints());
        }

        private void OnUpgradeClicked()
        {
            if (_skillManager != null)
            {
                _skillManager.RPC_RequestUpgradeSkill(_slotIndex);
            }
        }
    }
}