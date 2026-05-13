using UnityEngine;
using UnityEngine.UI;
using Skills;
using TMPro;
using Local.Progression;

namespace UI
{
    public class SkillSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _cooldownOverlay; // Dark, Counter-Clockwise
        [SerializeField] private Image _activeOverlay;   // Light, Clockwise
        
        [Header("Text & Indicators")]
        [SerializeField] private TextMeshProUGUI _cooldownText; // Displays remaining time (Active or CD)
        [SerializeField] private TextMeshProUGUI _chargesText;  // Displays numeric charges
        [SerializeField] private GameObject _upgradeIndicator;  // The green '+' icon/button

        private NetworkSkill _trackedSkill;
        private NetworkLevelSystem _levelSystem;
        
        public void AssignSkill(NetworkSkill skill, NetworkLevelSystem levelSystem)
        {
            _trackedSkill = skill;
            _levelSystem = levelSystem;
            
            if (_trackedSkill != null && _trackedSkill.Data != null)
            {
                _iconImage.sprite = _trackedSkill.Data.Icon;
                _iconImage.enabled = true;
            }
            else
            {
                _iconImage.enabled = false;
                _cooldownOverlay.fillAmount = 0;
                _activeOverlay.fillAmount = 0;
                
                if (_cooldownText) _cooldownText.text = "";
                if (_chargesText) _chargesText.text = "";
            }
            
            if (_levelSystem != null)
            {
                _levelSystem.OnSkillPointsChanged += HandleSkillPointsChanged;
                HandleSkillPointsChanged(_levelSystem.GetSkillPoints()); 
            }
        }

        private void OnDestroy()
        {
            if (_levelSystem != null)
            {
                _levelSystem.OnSkillPointsChanged -= HandleSkillPointsChanged;
            }
        }

        private void HandleSkillPointsChanged(int points)
        {
            if (_upgradeIndicator != null)
            {
                _upgradeIndicator.SetActive(points > 0);
            }
        }

        private void Update()
        {
            if (_trackedSkill == null || !_trackedSkill.Object.IsValid) return;

            UpdateCooldownUI();
            UpdateActiveUI();
            UpdateChargesUI();
            UpdateTimeTextUI();
        }

        private void UpdateCooldownUI()
        {
            if (_trackedSkill.CooldownEnd.IsRunning)
            {
                float totalCooldown = _trackedSkill.Data.GetCooldown(_trackedSkill.CurrentLevel);
                float remaining = _trackedSkill.CooldownEnd.RemainingTime(_trackedSkill.Runner) ?? 0f;
                _cooldownOverlay.fillAmount = remaining / totalCooldown;
            }
            else
            {
                _cooldownOverlay.fillAmount = 0f;
            }
        }

        private void UpdateActiveUI()
        {
            // Reads the generic progress method (can be time-based or charge-based depending on the skill override)
            _activeOverlay.fillAmount = _trackedSkill.GetActiveProgress(_trackedSkill.Runner);
        }
        
        private void UpdateTimeTextUI()
        {
            if (_cooldownText == null) return;

            // Check Active/Channeling/Casting time first
            if (_trackedSkill.ActiveEnd.IsRunning)
            {
                float remaining = _trackedSkill.ActiveEnd.RemainingTime(_trackedSkill.Runner) ?? 0f;
                if (remaining > 0.05f) // Hide if practically 0
                {
                    _cooldownText.text = remaining.ToString("F1");
                    return;
                }
            }

            // If not active, check for Cooldown time
            if (_trackedSkill.CooldownEnd.IsRunning)
            {
                float remaining = _trackedSkill.CooldownEnd.RemainingTime(_trackedSkill.Runner) ?? 0f;
                if (remaining > 0.05f)
                {
                    _cooldownText.text = remaining.ToString("F1");
                    return;
                }
            }

            // Hide text if no timer is running or they reached zero
            _cooldownText.text = "";
        }

        private void UpdateChargesUI()
        {
            int currentCharges = _trackedSkill.GetCurrentCharges();
            
            if (currentCharges <= 0) 
            {
                if (_chargesText) _chargesText.text = "";
                return; 
            }

            if (_chargesText) 
                _chargesText.text = currentCharges.ToString();
        }
    }
}