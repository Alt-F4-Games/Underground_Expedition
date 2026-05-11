using UnityEngine;
using UnityEngine.UI;
using Skills;

namespace UI
{
    public class SkillSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _cooldownOverlay; // Dark, Counter-Clockwise
        [SerializeField] private Image _activeOverlay;   // Light, Clockwise
        
        [Header("Charges Layout")]
        [SerializeField] private Transform _chargesContainer; // Horizontal Layout Group
        [SerializeField] private GameObject _chargePrefab;

        private NetworkSkill _trackedSkill;
        private Image[] _chargeInstances;

        // Initialize the slot with a specific skill from the player
        public void AssignSkill(NetworkSkill skill)
        {
            _trackedSkill = skill;
            
            if (_trackedSkill != null && _trackedSkill.Data != null)
            {
                _iconImage.sprite = _trackedSkill.Data.Icon;
                _iconImage.enabled = true;
                SetupCharges();
            }
            else
            {
                _iconImage.enabled = false;
                _cooldownOverlay.fillAmount = 0;
                _activeOverlay.fillAmount = 0;
            }
        }

        private void SetupCharges()
        {
            // Clear previous charges
            foreach (Transform child in _chargesContainer) Destroy(child.gameObject);

            int maxCharges = _trackedSkill.GetMaxCharges();
            
            if (maxCharges > 0)
            {
                _chargeInstances = new Image[maxCharges];
                for (int i = 0; i < maxCharges; i++)
                {
                    GameObject chargeObj = Instantiate(_chargePrefab, _chargesContainer);
                    _chargeInstances[i] = chargeObj.GetComponent<Image>();
                }
            }
        }

        private void Update()
        {
            if (_trackedSkill == null || !_trackedSkill.Object.IsValid) return;

            UpdateCooldownUI();
            UpdateActiveUI();
            UpdateChargesUI();
        }

        private void UpdateCooldownUI()
        {
            if (_trackedSkill.CooldownEnd.IsRunning)
            {
                // Calculate percentage (0 to 1)
                float totalCooldown = _trackedSkill.Data.GetCooldown(_trackedSkill.CurrentLevel);
                float remaining = _trackedSkill.CooldownEnd.RemainingTime(_trackedSkill.Runner).Value;
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

        private void UpdateChargesUI()
        {
            int currentCharges = _trackedSkill.GetCurrentCharges();
            
            if (currentCharges < 0 || _chargeInstances == null) return; // Skill doesn't use charges

            for (int i = 0; i < _chargeInstances.Length; i++)
            {
                // Light up the square if we have the charge, darken it if consumed
                Color c = _chargeInstances[i].color;
                c.a = (i < currentCharges) ? 1f : 0.2f; 
                _chargeInstances[i].color = c;
            }
        }
    }
}